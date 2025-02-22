using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            compilarSoluciónToolStripMenuItem.Enabled = false;
            //inicializa la opcion de compilar como inhabilitada.
        }
        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog VentanaAbrir = new OpenFileDialog
            {
                Filter = "Texto|*.c"
            };
            if (VentanaAbrir.ShowDialog() == DialogResult.OK)
            {
                archivo = VentanaAbrir.FileName;
                using (StreamReader Leer = new StreamReader(archivo))
                {
                    richTextBox1.Text = Leer.ReadToEnd();
                }

            }
            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
            compilarSoluciónToolStripMenuItem.Enabled = true;
            //habilita la opcion compilar cuando se carga un archivo.
        }

        private void Guardar()
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog
            {
                Filter = "Texto|*.c"
            };
            if (archivo != null)
            {
                using (StreamWriter Escribir = new StreamWriter(archivo))
                {
                    Escribir.Write(richTextBox1.Text);
                }
            }
            else
            {
                if (VentanaGuardar.ShowDialog() == DialogResult.OK)
                {
                    archivo = VentanaGuardar.FileName;
                    using (StreamWriter Escribir = new StreamWriter(archivo))
                    {
                        Escribir.Write(richTextBox1.Text);
                    }
                }
            }
            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
        }
        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Guardar();

        }
        private void nuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            archivo = null;
        }
        private void guardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog
            {
                Filter = "Texto|*.c"
            };
            if (VentanaGuardar.ShowDialog() == DialogResult.OK)
            {
                archivo = VentanaGuardar.FileName;
                using (StreamWriter Escribir = new StreamWriter(archivo))
                {
                    Escribir.Write(richTextBox1.Text);
                }
            }
            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
        }
        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            compilarSoluciónToolStripMenuItem.Enabled = true;
            //habilita la opcion compilar cuando se realiza un cambio en el texto.
        }

        //////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////funciones del analisis lexico/////////////////////////
        ///
        private char Tipo_caracter(int caracter)
        {
            if (caracter >= 65 && caracter <= 90 || caracter >= 97 && caracter <= 122) { return 'l'; } //letra 
            else
            {
                if (caracter >= 48 && caracter <= 57) { return 'd'; } //digito 
                else
                {
                    switch (caracter)
                    {
                        case 10: return 'n'; //salto de linea
                        case 34: return '"';//inicio de cadena
                        case 39: return 'c';//inicio de caracter
                        case 47: return '/';//inicio de comentario de linea o de bloque
                        case 32: return 'e';//espacio
                        default: return 's';//simbolo
                    }

                }
            }

        }
        private void Simbolo()
        {
            if (i_caracter == 33 ||
                i_caracter >= 35 && i_caracter <= 38 ||
                i_caracter >= 40 && i_caracter <= 45 ||
                i_caracter >= 58 && i_caracter <= 62 ||
                i_caracter == 91 ||
                i_caracter == 93 ||
                i_caracter == 94 ||
                i_caracter == 123 ||
                i_caracter == 124 ||
            i_caracter == 125
                ) { elemento = elemento + (char)i_caracter + "\n"; } //simbolos validos 
            else { Error(i_caracter); }
        }
        private void Cadena()
        {
            do
            {
                i_caracter = Leer.Read();
                if (i_caracter == 10) Numero_linea++;
            } while (i_caracter != 34 && i_caracter != -1);
            if (i_caracter == -1) Error(34);
        }
        private void Caracter()
        {
            i_caracter = Leer.Read();
            //programar para los casos donde el caracter se imprime  '\n','\r','\t' etc.
            i_caracter = Leer.Read();
            if (i_caracter != 39) Error(39);
        }
        private void Error(int i_caracter)
        {
            Rtbx_salida.AppendText("Error léxico " + (char)i_caracter + ", línea " + Numero_linea + "\n");
            N_error++;
        }
        private void Archivo_Libreria()
        {
            i_caracter = Leer.Read();
            if ((char)i_caracter == 'h') { elemento = "Libreria\n"; i_caracter = Leer.Read(); }
            else { Error(i_caracter); }
        }
        private bool Palabra_Reservada()
        {
            if (P_Reservadas.IndexOf(elemento) >= 0) return true;
            return false;
        }
        private void Identificador()
        {
            do
            {
                elemento += (char)i_caracter;
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'l' || Tipo_caracter(i_caracter) == 'd');
            if (Palabra_Reservada()) elemento += "\n";
            else
            {
                switch (i_caracter)
                {
                    case '.': Archivo_Libreria(); break;
                    case '(': elemento = "funcion\n"; break;
                    default: elemento = "identificador\n"; break;
                }
            }
        }
        private bool Comentario()
        {
            i_caracter = Leer.Read();
            switch (i_caracter)
            {
                case 47: // Comentario de línea
                    do
                    {
                        i_caracter = Leer.Read();
                    } while (i_caracter != 10 && i_caracter != -1);
                    return true;

                case 42: // Comentario de bloque
                    do
                    {
                        do
                        {
                            i_caracter = Leer.Read();
                            if (i_caracter == 10) { Numero_linea++; }
                        } while (i_caracter != 42 && i_caracter != -1);

                        i_caracter = Leer.Read();
                    } while (i_caracter != 47 && i_caracter != -1);

                    if (i_caracter == -1)
                    {
                        Rtbx_salida.AppendText($"Error léxico: Comentario de bloque sin cerrar en línea {Numero_linea}\n");
                        N_error++;
                        return false;
                    }

                    return true;

                default: return false;
            }
        }

        private void Numero_Real()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');
            elemento = "numero_real\n";
        }
        private void Numero()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');
            if ((char)i_caracter == '.') { Numero_Real(); }
            else
            {
                elemento = "numero_entero\n";
            }
        }
        ///////////////////Inicio del analisis léxico////////////////////////////
        /////////////////////////////////////////////////////////////////////////
        private void compilarSoluciónToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rtbx_salida.Text = "Analizando...\n";
            Guardar(); elemento = "";
            N_error = 0; Numero_linea = 1;
            archivoback = archivo.Remove(archivo.Length - 1) + "back";
            Escribir = new StreamWriter(archivoback);
            Leer = new StreamReader(archivo);
            i_caracter = Leer.Read();
            do
            {
                elemento = "";
                switch (Tipo_caracter(i_caracter))
                {
                    case 'l': Identificador(); Escribir.Write(elemento); break;
                    case 'd': Numero(); Escribir.Write(elemento); break;
                    case 's': Simbolo(); Escribir.Write(elemento); i_caracter = Leer.Read(); break;
                    case '"': Cadena(); Escribir.Write("cadena\n"); i_caracter = Leer.Read(); break;
                    case 'c': Caracter(); Escribir.Write("caracter\n"); i_caracter = Leer.Read(); break;
                    case '/': if (Comentario()) { Escribir.Write("comentario\n"); } else { Escribir.Write("/\n"); } break;
                    case 'n': i_caracter = Leer.Read(); Numero_linea++; Escribir.Write("LF\n"); break;
                    case 'e': i_caracter = Leer.Read(); break;
                    default: Error(i_caracter); break;
                };

            } while (i_caracter != -1);
            Escribir.Write("Fin");
            Escribir.Close();
            Leer.Close();
            if (N_error == 0) { Rtbx_salida.AppendText("Errores Lexicos: " + N_error); A_Sintactico(); }
            else { Rtbx_salida.AppendText("Errores: " + N_error); }
        }

        //////////////////////////////////////////////////////////////////////////
        ////////////////////Funciones del análisis sintáctico///////////////////////////////////
        private void ErrorS(string e, string s)
        {
            Rtbx_salida.AppendText("Linea: " + Numero_linea + ". Error de sintaxis " + e + ", se esperaba " + s + "\n");
            token = ""; N_error++;
        }
        //----------------------------------------------------------------------------
        private void Include()
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "<":
                    token = Leer.ReadLine();
                    if (token == "Libreria")
                    {
                        token = Leer.ReadLine();
                        if (token == ">")
                        {
                            token = Leer.ReadLine();
                        }
                        else { ErrorS(token, ">"); N_error++; }
                    }
                    else { ErrorS(token, "nombre de archivo libreria"); N_error++; }
                    break;
                case "cadena": token = Leer.ReadLine(); break;
                //case "identificador": token = Leer.ReadLine(); break;
                default: ErrorS(token, "inclusión valida "); N_error++; break;
            }
        }

        private void Define()
        {
            token = Leer.ReadLine();  // Leer el siguiente token, debe ser un identificador (nombre de la macro)

            if (token == "identificador")
            {
                string macroName = token;  // Guardamos el nombre de la macro
                token = Leer.ReadLine();  // Leer el siguiente token, puede ser un valor o una expresión

                if (token != null && token != "EOF")
                {
                    // Si hay un valor o expresión válida, guardamos el macro
                    // Aquí deberías almacenar la macro en alguna estructura (tabla de símbolos)
                    Console.WriteLine($"Macro definida: {macroName} = {token}");
                }
                else
                {
                    ErrorS(token, "valor o expresión de la macro");  // Error si no hay valor o expresión
                }
            }
            else
            {
                ErrorS(token, "nombre de macro válido");  // Error si no es un identificador
            }
        }

        private void If()
        {
            token = Leer.ReadLine();  // Leer el token después de '#if'
            Console.WriteLine($"Token leído: {token}");

            if (token == null || token != "(")  // Verificar si falta el '('
            {
                ErrorS(token, "'(' esperado después de #if");
                return;
            }

            token = Leer.ReadLine();  // Leer la condición que debe ir dentro de los paréntesis
            if (token == null || token == "")  // Verificar que haya una expresión dentro de los paréntesis
            {
                ErrorS(token, "expresión condicional esperada dentro de los paréntesis");
                return;
            }

            // Intentar evaluar la expresión condicional
            if (int.TryParse(token, out int result))
            {
                if (result != 0)
                {
                    Console.WriteLine($"Condición #if evaluada a TRUE: {result}");
                }
                else
                {
                    Console.WriteLine($"Condición #if evaluada a FALSE: {result}");
                }
            }
            else
            {
                ErrorS(token, "expresión condicional válida esperada");
                return;
            }

            token = Leer.ReadLine();  // Leer el token después de la condición
            if (token == null || token != ")")  // Verificar que cierre con ')'
            {
                ErrorS(token, "')' esperado después de la expresión condicional");
                return;
            }

            token = Leer.ReadLine();  // Leer el token después de los paréntesis
            if (token != "{")  // Verificar que empiece el bloque con '{'
            {
                ErrorS(token, "'{' esperado para comenzar el bloque de código");
                return;
            }

            // Continuar procesando el bloque de código dentro de las llaves
            bool llaveCerrada = false;
            while ((token = Leer.ReadLine()) != null)
            {
                if (token == "}")
                {
                    llaveCerrada = true;
                    break; // Terminamos el bloque, encontramos el cierre de la llave
                }
            }

            if (!llaveCerrada)
            {
                ErrorS(token, "'}' esperado para cerrar el bloque de código");
            }
        }

        private void Else()
        {
            token = Leer.ReadLine();  // Leer la directiva #else
            if (token != "{")  // Verificar que el bloque de código comience con '{'
            {
                ErrorS(token, "'{' esperado para el bloque de código de #else");
                return;
            }

            // Continuar procesando el bloque de código del #else
            while ((token = Leer.ReadLine()) != "}")
            {
                if (token == null)
                {
                    ErrorS(token, "'}' esperado para cerrar el bloque de código de #else");
                    return;
                }

                // Aquí agregarías la lógica de procesamiento del bloque de código del #else
            }
        }


        //--------------------------------------------------------------------------
        private void Directriz()
        {
            token = Leer.ReadLine(); // Leemos la directiva (ejemplo: #if, #for, #switch)

            switch (token)
            {
                case "#include":
                    Include();
                    break;
                case "#define":
                    Define();
                    break;
                case "#if":
                    If();
                    break;
                case "#for":
                    For(); // Procesar #for
                    break;
                case "#switch":
                    Switch(); // Procesar #switch
                    break;
                case "error":    //estructura para directriz #error
                    break;
                default:
                    ErrorS(token, "Directriz de preprocesador desconocida");
                    break;
            }
        }
        //---------------------------------------------------------------------------
        private int Constante()
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "numero_real": return 1;
                case "numero_entero": return 1;
                case "caracter": return 1;
                case "identificador": return 1;
                default: return 0;
            }
        }
        //-----------------------------------------------------------------------------
        private void Bloque_Inicializacion()
        {
            do
            {
                token = Leer.ReadLine();
                if (token == "{")
                {
                    do
                    {
                        if (Constante() == 1) { token = "elemento"; }
                        switch (token)
                        {
                            case "elemento": token = Leer.ReadLine(); break;
                            case "{":
                                do
                                {
                                    if (Constante() == 0) { ErrorS(token, " inicializacion valida de arreglo."); }
                                    else { token = Leer.ReadLine(); }
                                } while (token == ",");
                                if (token == "}") { token = Leer.ReadLine(); }
                                else { ErrorS(token, "}"); }
                                break;
                        }
                    } while (token == ",");
                    if (token == "}") { token = Leer.ReadLine(); }
                    else { ErrorS(token, "}"); }
                }
                else { ErrorS(token, "{"); }
            } while (token == ",");
        }
        //-------------------------------------------------------------------------------
        private void D_Arreglos()
        {
            while (token == "[")
            {
                token = Leer.ReadLine();
                if (token == "identificador" || token == "numero_entero")
                {
                    token = Leer.ReadLine();
                    if (token == "]") { token = Leer.ReadLine(); }
                    else { ErrorS(token, "]"); }
                }
                else ErrorS(token, "valor de longitud");
            }
            switch (token)
            {
                case ";": token = Leer.ReadLine(); break;
                case "=":
                    token = Leer.ReadLine();
                    if (token == "{")
                    {
                        Bloque_Inicializacion();
                        if (token == "}")
                        {
                            token = Leer.ReadLine();
                            if (token == ";") { token = Leer.ReadLine(); }
                            else { ErrorS(token, ";"); }
                        }
                        else { ErrorS(token, "}"); }
                    }
                    else { ErrorS(token, "{"); }
                    break;
                default: ErrorS(token, "declaracion valida para arreglos."); break;
            }
        }
        //----------------------------------------------------------------------------
        private void Dec_VGlobal() //se ha leido tipo e identificador
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "=":
                    if (Constante() == 1)
                    {
                        token = Leer.ReadLine();
                        if (token == ";") { token = Leer.ReadLine(); }
                        else { ErrorS(token, ";"); }
                    }
                    else { ErrorS(token, "inicializacion global valida"); }
                    break;
                case "[": D_Arreglos(); break;
                case ";": token = Leer.ReadLine(); break;
                default: ErrorS(token, ";"); break;
            }
        }
        //--------------------------------------------------------------------------
        private void Declaracion()
        {
            switch (token)
            {
                case "identificador": Dec_VGlobal(); break;
                case "funcion": Dec_Funcion(); break;
                default: ErrorS(token, "declaracion global valida"); break;
            }
        }
        private void Dec_Funcion()
        {
            token = Leer.ReadLine();  // Leer el tipo de retorno de la función

            // Verificar que el tipo de retorno sea válido (puede ser int, void, etc.)
            if (P_Res_Tipo.Contains(token))
            {
                string tipoRetorno = token;  // Almacenamos el tipo de retorno

                token = Leer.ReadLine();  // Leer el nombre de la función

                // Verificar que el nombre de la función sea válido (usualmente un identificador)
                if (token == "identificador")
                {
                    string nombreFuncion = Leer.ReadLine();  // Leer el nombre de la función

                    // Esperamos el paréntesis de apertura '('
                    token = Leer.ReadLine();
                    if (token == "(")
                    {
                        // Procesar los parámetros
                        ProcesarParametros();

                        // Ahora esperamos el paréntesis de cierre ')'
                        token = Leer.ReadLine();
                        if (token == ")")
                        {
                            // Ahora esperamos la apertura del bloque de la función '{'
                            token = Leer.ReadLine();
                            if (token == "{")
                            {
                                // Leer el cuerpo de la función
                                ProcesarCuerpoFuncion();
                            }
                            else
                            {
                                ErrorS(token, "{");
                            }
                        }
                        else
                        {
                            ErrorS(token, ")");
                        }
                    }
                    else
                    {
                        ErrorS(token, "(");
                    }
                }
                else
                {
                    ErrorS(token, "nombre de la función válido");
                }
            }
            else
            {
                ErrorS(token, "tipo de retorno válido");
            }
        }
        private void ProcesarParametros()
        {
            token = Leer.ReadLine();
            if (token != ")") // Si no es el paréntesis de cierre
            {
                // Leemos el tipo del primer parámetro
                if (P_Res_Tipo.Contains(token))
                {
                    string tipoParametro = token;
                    token = Leer.ReadLine();  // Leer el nombre del parámetro

                    // Verificar si el nombre del parámetro es válido
                    if (token == "identificador")
                    {
                        token = Leer.ReadLine();  // Leer la coma (si hay más parámetros)
                        if (token == ",")
                        {
                            // Llamamos nuevamente a la función para procesar más parámetros
                            ProcesarParametros();
                        }
                    }
                    else
                    {
                        ErrorS(token, "nombre del parámetro válido");
                    }
                }
                else
                {
                    ErrorS(token, "tipo de parámetro válido");
                }
            }
        }

        private void ProcesarCuerpoFuncion()
        {
            token = Leer.ReadLine();  // Leer la primera línea del cuerpo de la función

            while (token != "}")
            {
                if (token == "identificador")
                {
                    // Lógica para procesar una declaración de variable o función
                    token = Leer.ReadLine();
                }
                else if (token == "return")
                {
                    // Lógica para manejar la sentencia de retorno
                    token = Leer.ReadLine();
                    if (token == ";")  // El valor de retorno es opcional dependiendo de la función
                    {
                        token = Leer.ReadLine();
                    }
                    else
                    {
                        ErrorS(token, ";");
                    }
                }
                else
                {
                    // Si encontramos un token no esperado, procesamos como error
                    ErrorS(token, "sentencia válida en el cuerpo de la función");
                }
            }
        }

        private void Struct()
        {
            token = Leer.ReadLine();  // Leer "struct"
            if (token != "struct")
            {
                ErrorS(token, "struct");
                return;
            }

            token = Leer.ReadLine();  // Leer el nombre de la estructura
            if (token != "identificador")
            {
                ErrorS(token, "nombre de estructura válido");
                return;
            }

            token = Leer.ReadLine();  // Leer la apertura de las llaves
            if (token != "{")
            {
                ErrorS(token, "{");
                return;
            }

            // Leer los campos de la estructura
            while ((token = Leer.ReadLine()) != "}")
            {
                if (token == null)
                {
                    ErrorS(token, "} esperado para cerrar la estructura");
                    return;
                }
                if (P_Res_Tipo.Contains(token))  // Tipo de campo de la estructura
                {
                    token = Leer.ReadLine();  // Leer el nombre del campo
                    if (token != "identificador")
                    {
                        ErrorS(token, "nombre de campo válido");
                    }
                }
                else
                {
                    ErrorS(token, "tipo de campo válido");
                }
            }

            Console.WriteLine($"Estructura definida: {token}");
        }

        // Manejar la declaración de un tipo enum
        private void Enum()
        {
            token = Leer.ReadLine();  // Leer "enum"
            if (token != "enum")
            {
                ErrorS(token, "enum");
                return;
            }

            token = Leer.ReadLine();  // Leer el nombre del enum
            if (token != "identificador")
            {
                ErrorS(token, "nombre de enum válido");
                return;
            }

            token = Leer.ReadLine();  // Leer la apertura de las llaves
            if (token != "{")
            {
                ErrorS(token, "{");
                return;
            }

            // Leer los valores del enum
            while ((token = Leer.ReadLine()) != "}")
            {
                if (token == null)
                {
                    ErrorS(token, "} esperado para cerrar el enum");
                    return;
                }

                if (token != "identificador")
                {
                    ErrorS(token, "valor de enum válido");
                }
            }

            Console.WriteLine($"Enum definido: {token}");
        }

        //----------------------------------------------------------------------
        private void While()
        {
            token = Leer.ReadLine();  // Leer "while"
            if (token != "while")
            {
                ErrorS(token, "while");
                return;
            }

            token = Leer.ReadLine();  // Leer la condición entre paréntesis
            if (token != "(")
            {
                ErrorS(token, "(");
                return;
            }

            token = Leer.ReadLine();  // Leer la expresión condicional
            if (token == null)
            {
                ErrorS(token, "expresión condicional esperada");
                return;
            }

            token = Leer.ReadLine();  // Leer el paréntesis de cierre
            if (token != ")")
            {
                ErrorS(token, ")");
                return;
            }

            token = Leer.ReadLine();  // Leer el bloque de código
            if (token != "{")
            {
                ErrorS(token, "{");
                return;
            }


            // Procesamos el cuerpo del bucle
            while ((token = Leer.ReadLine()) != "}")
            {
                if (token == null)
                {
                    ErrorS(token, "} esperado para cerrar el bloque");
                    return;
                }
                // Aquí puedes procesar las sentencias dentro del bucle
            }
        }

        // Sentencia do-while
        private void DoWhile()
        {
            token = Leer.ReadLine();  // Leer "do"
            if (token != "do")
            {
                ErrorS(token, "do");
                return;
            }

            // Procesar el cuerpo del do
            token = Leer.ReadLine();
            if (token != "{")
            {
                ErrorS(token, "{");
                return;
            }

            // Manejo del bloque interno
            int balance = 1;  // Iniciamos con una llave abierta
            while (balance > 0 && (token = Leer.ReadLine()) != null)
            {
                if (token == "{") balance++;
                if (token == "}") balance--;
            }
            if (balance != 0)
            {
                ErrorS(token, "Desbalance en los bloques { }");
                return;
            }

            token = Leer.ReadLine();  // Leer "while"


            while ((token = Leer.ReadLine()) != "}")
            {
                if (token == null)
                {
                    ErrorS(token, "} esperado para cerrar el bloque");
                    return;
                }
            }

            token = Leer.ReadLine();  // Leer "while"
            if (token != "while")
            {
                ErrorS(token, "while");
                return;
            }

            token = Leer.ReadLine();  // Leer la condición entre paréntesis
            if (token != "(")
            {
                ErrorS(token, "(");
                return;
            }

            token = Leer.ReadLine();  // Leer la expresión condicional
            if (token == null)
            {
                ErrorS(token, "expresión condicional esperada");
                return;
            }

            token = Leer.ReadLine();  // Leer el paréntesis de cierre
            if (token != ")")
            {
                ErrorS(token, ")");
                return;
            }
        }

        // Sentencia for
        private void For()
        {
            token = Leer.ReadLine();  // Leer la directiva #for

            if (token == null || token != "(")  // Verificar si falta el '('
            {
                ErrorS(token, "'(' esperado después de #for");
                return;
            }

            token = Leer.ReadLine();  // Leer la condición que debe ir dentro de los paréntesis
            if (token == null || token == "")  // Verificar que haya una expresión dentro de los paréntesis
            {
                ErrorS(token, "expresión condicional esperada dentro de los paréntesis");
                return;
            }

            // Intentar evaluar la expresión condicional (aquí asumimos que puede ser un número)
            if (int.TryParse(token, out int result))
            {
                // Evaluamos si el número es 0 (falso) o distinto de 0 (verdadero)
                if (result != 0)
                {
                    Console.WriteLine($"Condición #for evaluada a TRUE: {result}");
                    // Continuamos procesando el código si la condición es verdadera
                }
                else
                {
                    Console.WriteLine($"Condición #for evaluada a FALSE: {result}");
                    // Saltamos el bloque de código si la condición es falsa
                }
            }
            else
            {
                ErrorS(token, "expresión condicional válida esperada (se esperaba un número o una expresión válida)");
                return;
            }

            token = Leer.ReadLine();  // Leer el token después de la condición

            if (token == null || token != ")")  // Verificar que cierre con ')'
            {
                ErrorS(token, "')' esperado después de la expresión condicional");
                return;
            }

            token = Leer.ReadLine();  // Leer el token después de los paréntesis
            if (token != "{")  // Verificar que empiece el bloque con '{'
            {
                ErrorS(token, "'{' esperado para comenzar el bloque de código");
                return;
            }

            // Continuar procesando el bloque de código dentro de las llaves
            while ((token = Leer.ReadLine()) != "}")
            {
                if (token == null)
                {
                    ErrorS(token, "'}' esperado para cerrar el bloque de código de #for");
                    return;
                }

                // Aquí agregarías la lógica de procesamiento del bloque de código dentro del #for
            }
        }

        // Sentencia switch
        private void Switch()
        {
            token = Leer.ReadLine();  // Leer "switch"
            if (token != "switch")
            {
                ErrorS(token, "switch");
                return;
            }

            token = Leer.ReadLine();  // Leer la expresión entre paréntesis
            if (token != "(")
            {
                ErrorS(token, "(");
                return;
            }

            token = Leer.ReadLine();  // Leer la condición del switch
            if (token == null)
            {
                ErrorS(token, "expresión condicional esperada");
                return;
            }

            token = Leer.ReadLine();  // Leer el paréntesis de cierre
            if (token != ")")
            {
                ErrorS(token, ")");
                return;
            }

            token = Leer.ReadLine();  // Leer la apertura del bloque
            if (token != "{")
            {
                ErrorS(token, "{");
                return;
            }

            // Procesamos cada caso dentro del switch
            while ((token = Leer.ReadLine()) != "}")
            {
                if (token == null)
                {
                    ErrorS(token, "} esperado para cerrar el switch");
                    return;
                }
                // Aquí procesas los casos dentro del switch
            }
            if (token == "case" || token == "default")
            {
                // Procesar un caso
                token = Leer.ReadLine();  // Leer el valor del case
                if (token == null)
                {
                    ErrorS(token, "valor esperado después de 'case'");
                    return;
                }

                token = Leer.ReadLine();  // Leer ':'
                if (token != ":")
                {
                    ErrorS(token, ": esperado después del valor del case");
                    return;
                }
            }

        }
        //--------------------------------------------------------------------
        private void ExpresionCondicional()
        {
            if (token == "identificador" || token == "constante")
            {
                token = Leer.ReadLine();
                while (token == "==" || token == "!=" || token == ">" || token == "<")
                {
                    token = Leer.ReadLine();  // Leer el siguiente operando
                    if (token != "identificador" && token != "constante")
                    {
                        ErrorS(token, "identificador o constante esperado");
                        return;
                    }
                    token = Leer.ReadLine();
                }
            }

        }

        private void F_Main()
        {
            // Leer y verificar el tipo de retorno
            token = Leer.ReadLine();
            Console.WriteLine("Token leído: " + token);

            if (token != "int")
            {
                // Si el tipo de retorno no es "int", hay un error
                ErrorS(token, "int");
                return;  // Salir de la función si el tipo es incorrecto
            }

            // Leer y verificar el nombre de la función
            token = Leer.ReadLine();
            Console.WriteLine("Token leído: " + token);

            if (token != "main")
            {
                // Si el nombre de la función no es "main", hay un error
                ErrorS(token, "main");
                return;
            }

            // Leer y verificar el paréntesis de apertura
            token = Leer.ReadLine();
            Console.WriteLine("Token leído: " + token);

            if (token != "(")
            {
                // Error si no encontramos el paréntesis de apertura
                ErrorS(token, "(");
                return;
            }

            // Leer y verificar el paréntesis de cierre
            token = Leer.ReadLine();
            Console.WriteLine("Token leído: " + token);

            if (token != ")")
            {
                // Error si no encontramos el paréntesis de cierre
                ErrorS(token, ")");
                return;
            }

            // Leer y verificar la apertura del bloque '{'
            token = Leer.ReadLine();
            Console.WriteLine("Token leído: " + token);

            if (token != "{")
            {
                // Error si no encontramos '{'
                ErrorS(token, "{");
                return;
            }

            // Si todo está correcto, procesamos el cuerpo de la función main
            ProcesarCuerpoFuncion();
        }

        //-------------------------------------------------------------------------
        private int Cabecera()
        {
            token = Leer.ReadLine();
            do
            {
                if (P_Res_Tipo.IndexOf(token) >= 0) { token = "tipo"; }
                switch (token)
                {    //en este caso practico solamente se considera la directiva #include
                    case "#": Directriz(); break;
                    case "tipo":
                        token = Leer.ReadLine();
                        if (token == "main") return 1;
                        else Declaracion();
                        break;
                    case "comentario": token = Leer.ReadLine(); break;
                    case "typedef": //estructura typedef
                        break;
                    case "const": //estrucutura const
                        break;
                    case "extern": //estrucutura extern
                        break;
                    case "": token = Leer.ReadLine(); break;
                    case "LF": Numero_linea++; token = Leer.ReadLine(); break;
                    default: token = Leer.ReadLine(); break;

                }
            } while (token != "Fin" && token != "main");
            return 0;
        }


        ////////////inicio del análisis sintáctico// // // // // //
        private void A_Sintactico()
        {
            Rtbx_salida.AppendText("\nAnalizando sintaxis...\n");
            N_error = 0;
            Numero_linea = 1;
            Leer = new StreamReader(archivoback);

            // Se empieza leyendo la cabecera
            if (Cabecera() == 1)
            {
                F_Main();
            }
            else
            {
                ErrorS(token, "función main()");
            }

            // Aquí procesamos el código del archivo línea por línea
            while ((token = Leer.ReadLine()) != null)
            {
                // Si el token es una directiva, como #if, #include, #define, #for, #switch, etc.
                if (token.StartsWith("#"))
                {
                    Directriz(); // Esto manejará #if, #include, #define, #for, #switch, etc.
                }
                else
                {
                    // Si no es una directiva, procesamos el resto del código
                    // Aquí agregamos la lógica de análisis para cada parte del código.

                    // 1. Reconocer declaraciones de variables globales
                    if (token == "int" || token == "float" || token == "char" || token == "double")
                    {
                        Dec_VGlobal();
                    }
                    // 2. Declaración de arreglos
                    else if (token == "array")
                    {
                        D_Arreglos();
                    }
                    // 3. Sentencias de control de flujo como while, do-while, if, etc.
                    else if (token.StartsWith("while"))
                    {
                        While();
                    }
                    else if (token.StartsWith("do"))
                    {
                        DoWhile();
                    }
                    else if (token.StartsWith("if"))
                    {
                        If();
                    }
                    // 4. Declaraciones de funciones
                    else if (token == "funcion")
                    {
                        Dec_Funcion();
                    }
                    // 5. Manejo de estructuras como structs o enums
                    else if (token.StartsWith("struct"))
                    {
                        Struct();
                    }
                    else if (token.StartsWith("enum"))
                    {
                        Enum();
                    }
                    else
                    {
                        // Si no es un caso específico, se puede agregar manejo de errores
                        ErrorS(token, "declaración o sentencia válida");
                    }
                }
                // Aumentamos el número de la línea
                Numero_linea++;
            }

            // Al finalizar, mostramos la cantidad de errores sintácticos
            Rtbx_salida.AppendText("\nErrores sintácticos: " + N_error);
            Leer.Close();
        }
    }
}