namespace Scriptor.Models;

/// <summary>
/// Representa um par de colunas em uma FK composta.
/// </summary>
public class FieldsForeignKey
{
    /// <summary>
    /// ID da coluna local (nesta tabela).
    /// Apenas IDs são armazenados (sem referências de objeto) para simplificar serialização/desserialização.
    /// </summary>
    public Guid ColumnId { get; set; }

    /// <summary>
    /// ID da coluna referenciada (na tabela referenciada).
    /// Apenas IDs são armazenados (sem referências de objeto) para simplificar serialização/desserialização.
    /// </summary>
    public Guid ReferencedColumnId { get; set; }
}
