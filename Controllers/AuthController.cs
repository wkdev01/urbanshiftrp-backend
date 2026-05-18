using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UrbanShiftRP.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private UrbanShiftRPDb _db;
    private IConfiguration _config;

    public AuthController(UrbanShiftRPDb db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginReq req)
    {
        if(req.RobloxId == null || req.RobloxId == "")
        {
            return BadRequest("robloxId vazio");
        }

        var player = await _db.Players.FirstOrDefaultAsync(p => p.RobloxId == req.RobloxId);

        if(player == null)
        {
            player = new Player();
            player.RobloxId = req.RobloxId;
            player.Nome = req.RobloxId;
            player.CriadoEm = DateTime.Now;
            player.UltimoLogin = DateTime.Now;

            _db.Players.Add(player);
            await _db.SaveChangesAsync();
        }

        if(player.Banido == true)
        {
            return Unauthorized("voce esta banido");
        }

        player.UltimoLogin = DateTime.Now;
        await _db.SaveChangesAsync();

        var secret = _config["JWT_SECRET"];
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
    public string RobloxId { get; set; }
}
public class LogoutReq
{
    public int PlayerId { get; set; }
}
