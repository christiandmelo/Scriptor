using System.Collections.Generic;

namespace Scriptor.Utilities
{
    public static class Constants
    {
        // Palavras reservadas SQL (SQL Server + Oracle)
        public static readonly HashSet<string> RESERVED_WORDS = new HashSet<string>
        {
            // DDL
            "ADD", "ALTER", "CREATE", "DROP", "RENAME", "TRUNCATE",
            // DML
            "SELECT", "INSERT", "UPDATE", "DELETE", "FROM", "WHERE",
            // Clausulas
            "AND", "OR", "NOT", "IN", "EXISTS", "BETWEEN", "LIKE",
            "ORDER", "BY", "GROUP", "HAVING", "UNION", "DISTINCT",
            // Joins
            "JOIN", "LEFT", "RIGHT", "INNER", "OUTER", "ON",
            // Outros
            "TABLE", "INDEX", "VIEW", "PROCEDURE", "FUNCTION",
            "PRIMARY", "KEY", "FOREIGN", "UNIQUE", "DEFAULT",
            "NULL", "CONSTRAINT", "CHECK", "CASE", "WHEN", "THEN", "ELSE"
        };

        public static readonly char[] INVALID_NAME_CHARACTERS = new[]
        {
            ' ', '@', '#', '$', '%', '&', '*', '(', ')', '-', '+', '=',
            '[', ']', '{', '}', '|', '\\', ':', ';', '\"', '\'', '<', '>', ',', '.', '/'
        };

        // Tamanhos padrão
        public const int DEFAULT_STRING_SIZE = 255;
        public const int DEFAULT_DECIMAL_SIZE = 10;
        public const int DEFAULT_DECIMAL_PRECISION = 2;
    }
}
