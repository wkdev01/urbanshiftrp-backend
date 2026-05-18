using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace UrbanShiftRP.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private UrbanShiftRPDb _db;
    private IConfiguration _config;
    private IHttpClientFactory _httpFactory;

    public AuthController(UrbanShiftRPDb db, IConfiguration config, IHttpClientFactory httpFactory)
    {
        _db = db;
        _config = config;
        _httpFactory = httpFactory;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginReq req)
    {
        if(string.IsNullOrWhiteSpace(req.RobloxId))
        {
            return BadRequest("robloxId vazio");
        }

        if(!long.TryParse(req.RobloxId, out var robloxId))
        {
            return BadRequest("robloxId invalido");
        }

        var http = _httpFactory.CreateClient();
        var robloxResponse = await http.GetAsync($"https://users.roblox.com/v1/users/{robloxId}");
        if(!robloxResponse.IsSuccessStatusCode)
        {
            return BadRequest("robloxId nao encontrado");
        }

        var json = await robloxResponse.Content.ReadAsStringAsync();
        var robloxDoc = JsonDocument.Parse(json).RootElement;
        var robloxName = robloxDoc.GetProperty("name").GetString() ?? string.Empty;
        if(string.IsNullOrWhiteSpace(robloxName))
        {
            robloxName = req.RobloxId;
        }

        var player = await _db.Players.FirstOrDefaultAsync(p => p.RobloxId == req.RobloxId);

        if(player == null)
        {
            player = new Player();
            player.RobloxId = req.RobloxId;
            player.Nome = robloxName;
            player.CriadoEm = DateTime.Now;
            player.UltimoLogin = DateTime.Now;

            _db.Players.Add(player);
        }
        else
        {
            player.Nome = robloxName;
        }

        if(player.Banido == true)
        {
            return Unauthorized("voce esta banido");
        }

        player.UltimoLogin = DateTime.Now;
        await _db.SaveChangesAsync();

        var secret = _config["JWT_SECRET"];
        if(string.IsNullOrWhiteSpace(secret))
        {
            return StatusCode(500, "JWT_SECRET nao configurado");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>();
        claims.Add(new Claim("playerId", player.Id.ToString()));
        claims.Add(new Claim("robloxId", player.RobloxId));

        var jwt = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        // salva a sessao
        var sessao = new Sessao();
        sessao.PlayerId = player.Id;
        sessao.Token = token;
        sessao.Inicio = DateTime.Now;
        sessao.Ativa = true;

        _db.Sessoes.Add(sessao);
        await _db.SaveChangesAsync();

        return Ok(new { token = token, playerId = player.Id });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutReq req)
    {
        var sessao = await _db.Sessoes.FirstOrDefaultAsync(s => s.PlayerId == req.PlayerId && s.Ativa == true);

        if(sessao != null)
        {
            sessao.Ativa = false;
            sessao.Fim = DateTime.Now;
            await _db.SaveChangesAsync();
        }

        return Ok();
    }
}

public class LoginReq
{
    public string RobloxId { get; set; } = string.Empty;
}
public class LogoutReq
{
    public int PlayerId { get; set; }
}
