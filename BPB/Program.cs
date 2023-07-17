using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BPB
{
    internal class Program
    {

        private static Dictionary<string, object> Variables;


        static void Main(string[] args)
        {
            // Dauerschleife
            while (true)
            {
                // Eingabe lesen
                string mycommand = Console.ReadLine();

                // Beenden wenn der Befehl "exit" eingegeben wurde
                if (mycommand == "exit") break;

                // neue Lexer Klasse 
                Lexer plex = new Lexer(mycommand);

                //gesammelte Eingaben verarbeiten aus Lexer
                Parser pParser = new Parser(plex.MeineTokens());
            }



        }
    }

    /// <summary>
    /// Definitionen der Token-Typen
    /// </summary>
    public enum TTYPES
    {
        NONE,
        IDENT,
        LIST,
        PRINT,
        STRING,
        NUMBER,
    }

    /// <summary>
    /// Token Model / Konstrukt
    /// </summary>
    public class Token
    {

        private TTYPES _Ttype;
        private string _value;

        public string Value
        {
            get => _value;
            set => _value = value;
        }
        public TTYPES TokenType
        {
            get => _Ttype;
            set => _Ttype = value;
        }
        public Token(TTYPES TypeOfToken, string Value = "")
        {
            this._value = Value;
            this._Ttype = TypeOfToken;
        }
        public override string ToString() => $"TOKEN => {this._Ttype} : {this._value}";

    }

    /// <summary>
    /// Lexer um die Eingaben zu verarbeiten
    /// </summary>
    public class Lexer
    {

        int pos = 0;
        string Src = string.Empty;
        List<Token> Tokens;

        public Lexer(string source)
        {
            Src = source;
            Tokens = new List<Token>();
            while (pos < Src.Length)
            {

                if (char.IsLetter(Src[pos])) // Wenn Zeichen = Buchstabe 
                {
                    string id = GetWord(); // Identifikator auslesen
                    TTYPES selType = TTYPES.IDENT; // Vorerste ein Identifikator
                    switch (id)
                    {
                        case "list":
                            selType = TTYPES.LIST; // Schlüsselwort...
                            break;
                        case "print":
                            selType = TTYPES.PRINT;
                            break;
                            /* Weitere Kommandos */

                    }
                    Tokens.Add(new Token(selType, id)); // In die Liste aufnehmen
                }
                else if (char.IsDigit(Src[pos])) // Zahlen auslesen ( Vorzeichen noch nicht verfügbar )
                    Tokens.Add(new Token(TTYPES.NUMBER, GetNumber()));
                else // Wenn nichts zutrifft 
                {
                    switch (Src[pos])  // Zeichen zuordnen 
                    {
                        case '\"':
                            pos++;
                            Tokens.Add(new Token(TTYPES.STRING, GetString()));
                            break;
                    }
                }
                pos++;
            }
        }
        public List<Token> MeineTokens() => Tokens; // Funktion um die Liste der Tokens wiederzugeben

        string GetString() // Funktion zum auslesen von Zeichenketten mit Doppelquote
        {
            string tString = string.Empty;
            bool terminated = false;
            while (pos < Src.Length)
            {
                if (Src[pos] == '\"')
                {
                    terminated = true;
                    break;
                }

                tString += Src[pos++];
            }


            if (!terminated)
                throw new Exception("Invalid String found.");

            return tString;

        }
        string GetNumber() // Nummern auslesen , von Ganzzahl bis Kommazahl
        {
            bool decP = false;
            string number = string.Empty;
            while (char.IsDigit(Src[pos]) || Src[pos] == '.')
            {
                if (Src[pos] == '.')
                {
                    if (decP)
                        throw new Exception("Invalid Number found.");
                    else decP = true;
                }
                number += Src[pos++];
                if (pos >= Src.Length) break;
            }
            return number;
        }
        string GetWord() // Identifikatoren auslesen
        {
            string word = string.Empty;

            while (char.IsLetterOrDigit(Src[pos]) || Src[pos] == '_')
            {
                word += Src[pos++];
                if (pos >= Src.Length) break;
            }



            return word;

        }
    }


    /// <summary>
    /// Die Parser Klasse um die vom Lexer gesammelten Infos zu verarbeiten
    /// noch sehr primitiv...
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Liste der gültigen Schlüsselwörter
        /// </summary>
        private List<TTYPES> Keywords = new List<TTYPES>()
        {
            TTYPES.LIST,
            TTYPES.PRINT
        };
        List<Token> MyTokens; // Liste der Tokens

        public Parser(List<Token> myTokens)
        {
            MyTokens = myTokens;



            // Überprüfung der Syntax ( <Schlüsselwort> [Parameter,...] )
            if (MyTokens.Count > 0)
            {
                if (!Keywords.Contains(MyTokens[0].TokenType))
                {
                    Console.WriteLine($"Expected a Keyword but found '{MyTokens[0].TokenType}.");
                    return;
                }

                for (int j = 1; j < MyTokens.Count(); j++)
                {
                    foreach (TTYPES K in Keywords)
                    {
                        if (K == MyTokens[j].TokenType)
                        {
                            Console.WriteLine($"Invalid use of '{K}'.");
                            break;
                        }

                    }
                }
            }
            else
                return;

            // Prüfen um welches Schlüsselwort es sich handelt
            switch (MyTokens[0].TokenType)
            {
                case TTYPES.PRINT: // Ausgeben von Infos also einfach die Liste abarbeiten


                    for (int i = 1; i < MyTokens.Count; i++)
                        Console.WriteLine($"{MyTokens[i].Value}:({MyTokens[i].TokenType})");

                    break;

                case TTYPES.LIST: // Gibt eine Liste von Dateien in einem Pfad aus
                    // Syntax : list <Name der Liste:String> <Pfad:String>

                    if (MyTokens.Count < 2)
                    {
                        Console.WriteLine("Invalid count of Arguments for 'list'.");
                        return;
                    }

                    if (MyTokens[1].TokenType != TTYPES.STRING)
                    {
                        Console.WriteLine($"Expected String Argument but got {MyTokens[1].TokenType}.");
                        return;
                    }


                    if (MyTokens[2].TokenType != TTYPES.STRING)
                    {
                        Console.WriteLine($"Expected String Argument but got {MyTokens[2].TokenType}.");
                        return;
                    }


                    Console.WriteLine($"[LISTNAME : {MyTokens[1].Value}\n");
                    string path = MyTokens[2].Value;
                    if (!Directory.Exists(path))
                        Console.WriteLine($"Invalid Path : '{path}.\n");
                    else
                    {
                        List<string> xP = Directory.GetFiles(path).ToList();
                        xP.ForEach(x => Console.WriteLine(x));
                    }


                    break;
            }
        }
    }
}
