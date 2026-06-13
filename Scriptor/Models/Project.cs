namespace Scriptor.Models;

/// <summary>
/// Representa um projeto completo com informações do cliente.
/// </summary>
public class Project
{
    /// <summary>
    /// Nome do cliente/empresa.
    /// </summary>
    public string NameClient { get; set; } = "";

    /// <summary>
    /// Nome do projeto de banco de dados.
    /// </summary>
    public string NameProject { get; set; } = "";

    /// <summary>
    /// Lista de tabelas (inicializada para evitar null reference).
    /// </summary>
    public List<Table> Tables { get; set; } = new List<Table>();
}
