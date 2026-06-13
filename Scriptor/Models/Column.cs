namespace Scriptor.Models;

/// <summary>
/// Representa uma coluna de uma tabela.
/// </summary>
public class Column
{
    /// <summary>
    /// Identificador único da coluna.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Nome da coluna.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Tipo de dado neutro (STRING, INTEGER, DECIMAL, DATE, BOOLEAN, TEXT, BLOB).
    /// Usado como string para flexibilidade e facilitar serialização.
    /// </summary>
    public string DataType { get; set; } = "";

    /// <summary>
    /// Tamanho (ex: VARCHAR(100), DECIMAL(10,2)).
    /// Nullable porque nem todos os tipos precisam de tamanho definido.
    /// </summary>
    public int? Size { get; set; }

    /// <summary>
    /// Precisão decimal (ex: casas decimais em DECIMAL).
    /// Nullable porque nem todos os tipos precisam de precisão definida.
    /// </summary>
    public int? Precision { get; set; }

    /// <summary>
    /// Se é parte da chave primária.
    /// </summary>
    public bool IsPrimaryKey { get; set; } = false;

    /// <summary>
    /// Se permite NULL.
    /// </summary>
    public bool AllowNull { get; set; } = true;
}
