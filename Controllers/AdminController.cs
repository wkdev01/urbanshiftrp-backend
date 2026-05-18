using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace UrbanShiftRP.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private UrbanShiftRPDb _db;
    private IConfiguration _config;

    public AdminController(UrbanShiftRPDb db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // nao tem autenticação aqui ainda to deixando pra depois
    [HttpGet("players")]
    public async Task<IActionResult> GetPlayers()
    {
        var players = await _db.Players.ToListAsync();
        return Ok(players);
    }

    [HttpGet("players/{id}")]
    public async Task<IActionResult> GetPlayer(int id)
    {
        var player = await _db.Players.FindAsync(id);
        if(player == null) return NotFound();

        var eventos = await _db.Eventos.Where(e => e.PlayerId == id).ToListAsync();
        var transacoes = await _db.Transacoes.Where(t => t.PlayerId == id).ToListAsync();

        return Ok(new
        {
            player = player,
            eventos = eventos,
            transacoes = transacoes
        });
    }

    [HttpPost("ban/{id}")]
    public async Task<IActionResult> Banir(int id, [FromBody] BanReq req)
    {
        var player = await _db.Players.FindAsync(id);
        if(player == null) return NotFound();

        player.Banido = true;
        player.MotivoBan = req.Motivo;

        var log = new BanLog();
        log.PlayerId = id;
        log.Motivo = req.Motivo;
        log.Data = DateTime.Now;

        _db.BanLogs.Add(log);
        await _db.SaveChangesAsync();

        // manda no discord
        var webhook = _config["DISCORD_WEBHOOK"];
        if(webhook != null && webhook != "")
        {
            try
            {
                var http = new HttpClient();
                var msg = $"{{\"content\": \"player {player.Nome} banido. motivo: {req.Motivo}\"}}";
                var content = new StringContent(msg, System.Text.Encoding.UTF8, "application/json");
                await http.PostAsync(webhook, content);
            }
            catch(Exception ex)
            {
                Console.WriteLine("erro discord: " + ex.Message);
            }
        }

        return Ok();
    }

    [HttpDelete("ban/{id}")]
    public async Task<IActionResult> RemoverBan(int id)
    {
        var player = await _db.Players.FindAsync(id);
        if(player == null) return NotFound();

        player.Banido = false;
        player.MotivoBan = null;
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("eventos")]
    public async Task<IActionResult> GetEventos()
    {
        // pega os ultimos 100 eventos
        var evs = await _db.Eventos
            .OrderByDescending(e => e.Data)
            .Take(100)
            .ToListAsync();

        return Ok(evs);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalPlayers = await _db.Players.CountAsync();
        var totalBanidos = await _db.Players.CountAsync(p => p.Banido == true);
        var totalEventosHoje = await _db.Eventos.CountAsync(e => e.Data >= DateTime.Today);

        return Ok(new
        {
            totalPlayers = totalPlayers,
            banidos = totalBanidos,
            eventosHoje = totalEventosHoje
        });
    }
}

public class BanReq
{
    public string Motivo { get; set; }
}
