namespace Scriptor.Models;

/// <summary>
/// Representa uma chave estrangeira (relacionamento).
/// </summary>
public class ForeignKey
{
    /// <summary>
    /// ID da tabela referenciada.
    /// Permite auto-relacionamento (tabela referencia a si mesma).
    /// </summary>
    public Guid ReferencedTableId { get; set; }

    /// <summary>
    /// Lista de pares campo-local → campo-remoto.
    /// Suporta FK composta (múltiplas colunas).
    /// Inicializada para evitar null reference.
    /// </summary>
    public List<FieldsForeignKey> FieldsForeignKey { get; set; } = new List<FieldsForeignKey>();
}
