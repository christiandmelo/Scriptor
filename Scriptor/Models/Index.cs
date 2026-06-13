namespace Scriptor.Models;

/// <summary>
/// Representa um índice na tabela.
/// </summary>
public class Index
{
    /// <summary>
    /// Nome do índice (ex: "IDX_User_Email").
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Colunas que compõem o índice.
    /// Armazena referências aos objetos Column, não IDs.
    /// Na serialização JSON, apenas IDs são salvos (custom converter).
    /// </summary>
    public List<Column> Columns { get; set; } = new List<Column>();
}
