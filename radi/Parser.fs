module Parser

open FParsec

type UserState = unit
type Parser<'t> = Parser<'t, UserState>

let pair x y =
    (x, y)

let triple x y z =
    (x, y, z)

let quadriple x y z w =
    (x, y, z, w)

let (>/) x y = x
let (/>) x y = y

// TODO: delim must be in all keywords
let delimMustBe : Parser<_> = pstring " " <|> pstring "\t" <|> (newline |>> (>/) " ") <|> (eof |>> (>/) " ")

//Parsing string in between ""
let private stringLiteral =
    let normalChar = satisfy (fun c -> c <> '\\' && c <> '"')
    let unescape c = match c with
                     | 'n' -> '\n'
                     | 'r' -> '\r'
                     | 't' -> '\t'
                     | c   -> c
    let escapedChar = pstring "\\" >>. (anyOf "\\nrt\"" |>> unescape)
    between (pstring "\"") (pstring "\"")
            (manyChars (normalChar <|> escapedChar))


let astOper, astOperRef = createParserForwardedToRef<AST.Expression, unit>()

//Helper parsers
let private _ID : Parser<_> = many1Satisfy2 (fun x -> System.Char.IsLetter x || x = '_') (fun x -> System.Char.IsLetterOrDigit x || x = '_') .>> spaces |>> AST.ID

//Basic types parsers
let private _Int : Parser<_> = pint32 .>> spaces |>> AST.Int
let private _Float : Parser<_> =
    pipe2 (manySatisfy (System.Char.IsDigit) .>> pstring ".") (manySatisfy (System.Char.IsDigit) .>> spaces) (
        fun x y -> AST.Float(float (x + "." + y))
    )
let private _String : Parser<_> = stringLiteral .>> spaces |>> AST.String
let private _Void : Parser<_> = pstring "void" >>. spaces |>> AST.Void
let private _Var : Parser<_> = many1Satisfy2 (System.Char.IsLetter) (System.Char.IsLetterOrDigit) .>> spaces |>> AST.Var

//Let parser
let private _Let : Parser<_> = pipe3 (pstring "let" >>. spaces >>. _ID) (pstring ":" >>. spaces >>. astOper .>> spaces) (astOper .>> spaces) (
    fun id expr1 expr2 -> AST.Let(id, expr1, expr2)
)

//Unary operators
let private Decrement : Parser<_> = stringReturn "~" "~" .>> spaces

let private AllUnaryOperators : Parser<_> = 
    Decrement

//Binary operators
let private Equal : Parser<_> = stringReturn "=" "=" .>> spaces
let private NotEqual : Parser<_> = stringReturn "!=" "!=" .>> spaces
let private Less : Parser<_> = stringReturn "<" "<" .>> spaces
let private Greater : Parser<_> = stringReturn ">" ">" .>> spaces
let private LessOrEqual : Parser<_> = stringReturn "<=" "<=" .>> spaces
let private GreaterOrEqual : Parser<_> = stringReturn ">=" ">=" .>> spaces
let private Plus : Parser<_> = stringReturn "+" "+" .>> spaces
let private Minus : Parser<_> = stringReturn "-" "-" .>> spaces
let private Mult : Parser<_> = stringReturn "*" "*" .>> spaces
let private Div : Parser<_> = stringReturn "/" "/" .>> spaces

let private AllBinaryOperators : Parser<_> = 
    LessOrEqual <|> GreaterOrEqual <|> Equal <|> NotEqual <|> Less <|> Greater <|>
    Plus <|> Minus <|> Mult <|> Div

//Operators parsers
let private _UnaryOperator : Parser<_> = 
    pipe2 (attempt AllUnaryOperators) (astOper .>> spaces) (
        fun id expr -> AST.UnaryOperator(id, expr)
    )

let private _BinaryOperator : Parser<_> = 
    pipe3 (attempt AllBinaryOperators) (astOper .>> spaces) (astOper .>> spaces) (
        fun id expr1 expr2 -> AST.BinaryOperator(id, expr1, expr2)
    )

//Apply parser
let private _Apply : Parser<_> = 
    pipe2 (pstring "app" >>. spaces >>. astOper) (astOper .>> spaces) (
        fun func x -> AST.Apply(func, x)
    )

//Lambda parser
let private _Lambda : Parser<_> =
    pipe2 (pstring "lambda" >>. spaces >>. _ID) (astOper .>> spaces) (
        fun id expr -> AST.Lambda(id, expr)
    )

//If parser
let private _If =
    pipe3 (pstring "if" >>. spaces >>. astOper .>> spaces) (astOper .>> spaces) (pstring "else" >>. spaces >>. astOper .>> spaces) (
        fun expr1 expr2 expr3 -> AST.If(expr1, expr2, expr3)
    )

//Chain
let private _Chain = 
    pipe2 (pstring "chain" >>. spaces >>. sepBy (astOper) (spaces >>. pstring "$" .>> spaces)) (pstring "->" >>. spaces >>. astOper .>> spaces) pair |>> AST.Chain

//Input and outpur parsers
let private _Print : Parser<_> = pstring "io.Print" >>. spaces >>. astOper .>> spaces |>> AST.Print
let private _ReadInt : Parser<_> = pstring "io.ReadInt" >>. spaces |>> AST.ReadInt
let private _ReadString : Parser<_> = pstring "io.ReadString" >>. spaces |>> AST.ReadString
let private _ReadFloat : Parser<_> = pstring "io.ReadFloat" >>. spaces |>> AST.ReadFloat
let private _NewLine : Parser<_> = pstring "io.NewLine" >>. spaces |>> AST.NewLine

//Math functions
let _MathFunc1 = pipe2 (pstring "math1." >>. _ID) (astOper .>> spaces) (
    fun func x -> AST.MathFunc1(func, x)
)

let private _MathFunc2 = pipe3 (pstring "math2." >>. _ID) (astOper .>> spaces) (astOper .>> spaces) (
    fun func x y -> AST.MathFunc2(func, x, y)
)

// Lazy
let private _Lazy = (pstring "lazy" >>. delimMustBe >>. spaces >>. astOper .>> spaces) |>> AST.Lazy
let private _Force = (pstring "force" >>. delimMustBe >>. spaces >>. astOper .>> spaces) |>> AST.Force

//List & List functions
let private _List = between (pstring "[" >>. spaces) (pstring "]" >>. spaces) (sepBy astOper (spaces >>. pstring "," .>> spaces)) |>> AST.List
let private _ListFunc1 = pipe2 (pstring "list1." >>. _ID .>> spaces) (astOper .>> spaces) pair |>> AST.ListFunc1
let private _ListFunc2 = pipe3 (pstring "list2." >>. _ID .>> spaces) (astOper .>> spaces) (astOper .>> spaces) triple |>> AST.ListFunc2
let private _ListFunc3 = pipe4 (pstring "list3." >>. _ID .>> spaces) (astOper .>> spaces) (astOper .>> spaces) (astOper .>> spaces) quadriple |>> AST.ListFunc3

// Util functions
let private _UtilFunc1 = pipe2 (pstring "util1." >>. _ID .>> spaces) (astOper .>> spaces) pair |>> AST.UtilFunc1

astOperRef.Value <-
    choice [
        attempt _Float
        _List
        attempt _ListFunc1 <|> _ListFunc2 <|> _ListFunc3
        attempt _UtilFunc1
        _Int
        _String
        attempt _Let
        _UnaryOperator
        _BinaryOperator
        attempt _Apply
        attempt _Lambda
        attempt _If
        attempt _NewLine
        attempt _Print
        attempt _ReadInt
        attempt _ReadString
        attempt _ReadFloat
        attempt _MathFunc1 <|> _MathFunc2
        attempt _Lazy
        attempt _Force
        attempt _Chain
        attempt _Void
        _Var
    ]

//Final parser
let private _Result : Parser<_> = spaces >>. astOper .>> eof

let ParseString (s : string): Result<AST.Expression, string> =
    match run _Result s with 
     | Success(res, _, _) -> Result.Ok res
     | Failure(err, _, _) -> Result.Error err
