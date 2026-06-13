namespace Scriptor.Models;

/// <summary>
/// Representa uma tabela no banco de dados.
/// </summary>
public class Table
{
    /// <summary>
    /// Identificador único da tabela (gerado automaticamente).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Nome da tabela.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Descrição/documentação da tabela (opcional).
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Colunas da tabela.
    /// </summary>
    public List<Column> Columns { get; set; } = new List<Column>();

    /// <summary>
    /// Índices da tabela.
    /// </summary>
    public List<Index> Indexes { get; set; } = new List<Index>();

    /// <summary>
    /// Chaves estrangeiras da tabela.
    /// </summary>
    public List<ForeignKey> Fks { get; set; } = new List<ForeignKey>();
}
