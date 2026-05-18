namespace UrbanShiftRP;

public class Player
{
    public int Id { get; set; }
    public string RobloxId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public decimal Coins { get; set; } = 0;
    public bool Banido { get; set; } = false;
    public string MotivoBan { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime UltimoLogin { get; set; }
    public int Mortes { get; set; } = 0;
    public int Kills { get; set; } = 0;
}

public class Sessao
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Inicio { get; set; } = DateTime.Now;
    public DateTime? Fim { get; set; }
    public bool Ativa { get; set; } = true;
}

public class Evento
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Dados { get; set; } = string.Empty;
    public DateTime Data { get; set; } = DateTime.Now;
}

public class Transacao
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty;   // "ganho" ou "gasto"
    public DateTime Data { get; set; } = DateTime.Now;
}

//sistema de ban
public class BanLog
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime Data { get; set; } = DateTime.Now;
}
