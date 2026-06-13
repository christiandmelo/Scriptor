using System;
using System.Collections.Generic;
using Scriptor.Models;

namespace Scriptor.Services
{
    public class ValidationService
    {
        /// <summary>
        /// Valida nome de tabela/coluna/índice
        /// Regras: sem espaços, sem caracteres especiais (exceto _), não é palavra reservada
        /// </summary>
        public static bool ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Padrão: apenas letras, números, underscore
            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9_]+$"))
                return false;

            // Não pode ser palavra reservada
            if (IsNameReservedWord(name.ToUpper()))
                return false;

            return true;
        }

        public static bool IsNameReservedWord(string name)
        {
            var reservedWords = Constants.RESERVED_WORDS;
            return reservedWords.Contains(name);
        }

        /// <summary>
        /// Verifica em quais índices a coluna está sendo usada
        /// </summary>
        public static List<Index> CheckColumnUsageInIndexes(Table table, Column column)
        {
            if (table == null || column == null)
                throw new ArgumentNullException(nameof(table), nameof(column));

            return table.Indexes
                .Where(idx => idx.Columns.Any(col => col.Id == column.Id))
                .ToList();
        }

        /// <summary>
        /// Verifica em quais FKs a coluna está sendo usada
        /// </summary>
        public static List<ForeignKey> CheckColumnUsageInForeignKeys(Table table, Column column)
        {
            if (table == null || column == null)
                throw new ArgumentNullException(nameof(table), nameof(column));

            return table.Fks
                .Where(fk => fk.FieldsForeignKey.Any(f => f.ColumnId == column.Id))
                .ToList();
        }

        /// <summary>
        /// Valida que duas colunas têm o mesmo tipo de dado (para FK)
        /// </summary>
        public static bool ValidateForeignKeyMapping(Column localColumn, Column referencedColumn)
        {
            if (localColumn == null || referencedColumn == null)
                throw new ArgumentNullException(nameof(localColumn), nameof(referencedColumn));

            // Tipos devem ser iguais
            if (localColumn.DataType != referencedColumn.DataType)
                return false;

            // Tamanho deve ser compatível
            if (localColumn.DataType == "STRING" || localColumn.DataType == "DECIMAL")
            {
                if (localColumn.Size != referencedColumn.Size)
                    return false;

                if (localColumn.DataType == "DECIMAL" && localColumn.Precision != referencedColumn.Precision)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Verifica se todas as referências de FK são válidas
        /// </summary>
        public static void ValidateForeignKeyConsistency(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            foreach (var table in project.Tables)
            {
                var invalidFks = new List<ForeignKey>();

                foreach (var fk in table.Fks)
                {
                    // Verificar se tabela referenciada existe
                    var referencedTable = project.Tables.FirstOrDefault(t => t.Id == fk.ReferencedTableId);
                    if (referencedTable == null)
                    {
                        invalidFks.Add(fk);
                        continue;
                    }

                    // Verificar se campos existem
                    var invalidFields = new List<FieldsForeignKey>();
                    foreach (var field in fk.FieldsForeignKey)
                    {
                        var localColumn = table.Columns.FirstOrDefault(c => c.Id == field.ColumnId);
                        var refColumn = referencedTable.Columns.FirstOrDefault(c => c.Id == field.ReferencedColumnId);

                        if (localColumn == null || refColumn == null)
                            invalidFields.Add(field);
                    }

                    // Remover campos órfãos
                    foreach (var field in invalidFields)
                        fk.FieldsForeignKey.Remove(field);

                    // Se FK ficou vazia, remover
                    if (fk.FieldsForeignKey.Count == 0)
                        invalidFks.Add(fk);
                }

                // Remover FKs inválidas
                foreach (var fk in invalidFks)
                    table.Fks.Remove(fk);
            }
        }
    }
}
