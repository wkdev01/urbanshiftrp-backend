using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace UrbanShiftRP.Controllers;

[ApiController]
[Route("player")]
public class PlayerController : ControllerBase
{
    private UrbanShiftRPDb _db;

    public PlayerController(UrbanShiftRPDb db)
    {
        _db = db;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlayer(int id)
    {
        var player = await _db.Players.FindAsync(id);

        if(player == null)
            return NotFound();

        return Ok(player);
    }

    [HttpGet("{id}/eventos")]
    public async Task<IActionResult> GetEventos(int id)
    {
        var evs = await _db.Eventos.Where(e => e.PlayerId == id).ToListAsync();
        return Ok(evs);
    }

    [HttpPost("{id}/evento")]
    public async Task<IActionResult> PostEvento(int id, [FromBody] EventoReq req)
    {
        var ev = new Evento();
        ev.PlayerId = id;
        ev.Tipo = req.Tipo;
        ev.Dados = req.Dados;
        ev.Data = DateTime.Now;

        _db.Eventos.Add(ev);
        await _db.SaveChangesAsync();

        // atualiza kills e mortes do player
        // BUG: se o player nao existir isso vai dar null reference
        var player = await _db.Players.FindAsync(id);

        if(req.Tipo == "kill")
        {
            player.Kills += 1;
            await _db.SaveChangesAsync();
        }
        if(req.Tipo == "morte")
        {
            player.Mortes += 1;
            await _db.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpGet("{id}/transacoes")]
    public async Task<IActionResult> GetTransacoes(int id)
    {
        var ts = await _db.Transacoes.Where(t => t.PlayerId == id).ToListAsync();
        return Ok(ts);
    }

    [HttpPost("{id}/transacao")]
    public async Task<IActionResult> PostTransacao(int id, [FromBody] TransacaoReq req)
    {
        if(req.Valor <= 0)
        {
            return BadRequest("valor invalido");
        }

        var player = await _db.Players.FindAsync(id);

        if(player == null)
            return NotFound();

        if(req.Tipo == "gasto")
        {
            if(player.Coins < req.Valor)
            {
                return BadRequest("coins insuficiente");
            }
            player.Coins -= req.Valor;
        }

        if (req.Tipo == "ganho")
        {
            player.Coins += req.Valor;
        }

        var t = new Transacao();
        t.PlayerId = id;
        t.Valor = req.Valor;
        t.Tipo = req.Tipo;
        t.Data = DateTime.Now;

        _db.Transacoes.Add(t);
        await _db.SaveChangesAsync();

        return Ok(new { coinsAtual = player.Coins });
    }
}

public class EventoReq
{
    public string Tipo { get; set; }
    public string Dados { get; set; }
}
public class TransacaoReq
{
    public decimal Valor { get; set; }
    public string Tipo { get; set; }
}
