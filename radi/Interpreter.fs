module Interpreter

// Interpreter

// Utils:
let unit x = ()

let (/>) x y = y

type private ErrorType =
    | InputError of unit
    | UndefinedStdFunction of unit

let private internalError errtype =
    match errtype with
    | InputError() -> AST.Void(printfn "%s" "radi internal error: Invalid input")
    | UndefinedStdFunction() -> AST.Void(printfn "%s" "radi internal error: Undefined std function")

let private unwrapInt y =
    match y with 
    | AST.Int(x) -> x

let private mergeMaps map1 map2 =
    Map.fold (fun acc key value -> Map.add key value acc) map1 map2

// Operators:
let rec private operator1 op a =
    match (op, a) with
    | ("~", AST.Int(x)) -> AST.Int(-x)
    | ("~", AST.Float(x)) -> AST.Float(-x)

let rec private operator2 op a b =
    match (op, a, b) with
    | ("+", AST.Int(x), AST.Int(y)) -> AST.Int(x + y)
    | ("-", AST.Int(x), AST.Int(y)) -> AST.Int(x - y)
    | ("*", AST.Int(x), AST.Int(y)) -> AST.Int(x * y)
    | ("/", AST.Int(x), AST.Int(y)) -> AST.Int(x / y)
    | ("+", AST.Float(x), AST.Float(y)) -> AST.Float(x + y)
    | ("-", AST.Float(x), AST.Float(y)) -> AST.Float(x - y)
    | ("*", AST.Float(x), AST.Float(y)) -> AST.Float(x * y)
    | ("/", AST.Float(x), AST.Float(y)) -> AST.Float(x / y)
    | ("<", AST.Int(x), AST.Int(y)) -> if x < y then AST.Int(1) else AST.Int(0)
    | (">", AST.Int(x), AST.Int(y)) -> if x > y then AST.Int(1) else AST.Int(0)
    | ("=", AST.Int(x), AST.Int(y)) -> if x = y then AST.Int(1) else AST.Int(0)
    | ("<=", AST.Int(x), AST.Int(y)) -> if x <= y then AST.Int(1) else AST.Int(0)
    | (">=", AST.Int(x), AST.Int(y)) -> if x >= y then AST.Int(1) else AST.Int(0)
    | ("<", AST.Float(x), AST.Float(y)) -> if x < y then AST.Int(1) else AST.Int(0)
    | (">", AST.Float(x), AST.Float(y)) -> if x > y then AST.Int(1) else AST.Int(0)
    | ("=", AST.Float(x), AST.Float(y)) -> if x = y then AST.Int(1) else AST.Int(0)
    | ("<=", AST.Float(x), AST.Float(y)) -> if x <= y then AST.Int(1) else AST.Int(0)
    | (">=", AST.Float(x), AST.Float(y)) -> if x >= y then AST.Int(1) else AST.Int(0)
    | ("+", AST.String(s1), AST.String(s2)) -> AST.String(s1 + s2)
    | ("+", AST.List(l1), AST.List(l2)) -> AST.List(List.append l1 l2)
    | (op, x, y) -> AST.Void(printfn "radi internal error: can't apply operand %s to %A and %A" op x y)

// Other functions:

let private mathFunc1 name x =
    match (name, x) with
    | ("Square", AST.Float(_x)) -> AST.Float(_x * _x)
    | ("Square", AST.Int(_x)) -> AST.Int(_x * _x)
    | ("Sqrt", AST.Float(_x)) -> AST.Float(sqrt(_x))
    | ("Sin", AST.Float(_x)) -> AST.Float(sin(_x))
    | ("Cos", AST.Float(_x)) -> AST.Float(cos(_x))
    | ("Abs", AST.Float(_x)) -> AST.Float(abs(_x))
    | ("Abs", AST.Int(_x)) -> AST.Int(abs(_x))
    | ("ToFloat", AST.Float(_x)) -> AST.Float(_x)
    | ("ToFloat", AST.Int(_x)) -> AST.Float(float(_x))
    | ("ToInt", AST.Float(_x)) -> AST.Int(int(_x))
    | ("ToInt", AST.Int(_x)) -> AST.Float(_x)
    | (f, x) -> AST.Void(printfn "radi internal error: can't apply math/1 %s to %A" f x)

let private mathFunc2 name x y =
    match (name, x, y) with
    | ("Pow", AST.Float(_x), AST.Float(_y)) -> AST.Float(_x ** _y)

// List functions:
let private listFunc1 name x =
    match (name, x) with
    | ("Length", AST.List(l)) -> AST.Int(List.length l)
    | ("Head", AST.List(l)) -> List.head l
    | ("Tail", AST.List(l)) -> AST.List(List.tail l)
    | ("IsEmpty", AST.List(l)) -> match l with | [] -> AST.Int(1) | _ -> AST.Int(0)
    | _ -> AST.Void(printfn "Can't apply func %s to %A" name x)

let private listFunc2 name x y evalFunc env =
    match (name, x, y) with
    | ("Map", f, AST.List(l)) -> AST.List(List.map (fun x -> evalFunc (AST.Apply(f, evalFunc x env)) env) l)
    | ("Reduce", f, AST.List(l)) ->
        List.reduce (fun x y -> evalFunc (AST.Apply(AST.Apply(f, x), y)) env) l
    | _ -> AST.Void(printfn "Can't apply func %s to %A and %A" name x y)

let private listFunc3 name x y z evalFunc env =
    match (name, x, y, z) with
    | ("Fold", f, st, AST.List(l)) ->
        List.fold (fun x y -> evalFunc (AST.Apply(AST.Apply(f, x), y)) env) st l
    | _ -> AST.Void(printfn "Can't apply func %s to %A, %A and %A" name x y z)

// Util:
let private utilFunc1 name x =
    match (name, x) with
    | ("ToString", AST.String(x)) -> AST.String(x)
    | ("ToString", AST.Int(x)) -> AST.String(sprintf "%d" x)
    | ("ToString", AST.Float(x)) -> AST.String(sprintf "%f" x)
    | _ -> AST.Void(printfn "Can't apply %s to %A" name x)

let rec Evaluate expr env =
    match expr with
    // Basic language rules:
    | AST.Int(n) -> AST.Int(n)
    | AST.Float(x) -> AST.Float(x)
    | AST.String(n) -> AST.String(n)
    | AST.Var("void") -> AST.Void()
    | AST.Var(id) -> Evaluate (Map.find id env) env
    | AST.Void() -> AST.Void()
    | AST.List(l) -> AST.List(List.map (fun x -> Evaluate x env) l)
    | AST.Let("void", exp1, exp2) ->
        let forwardedExpr = Evaluate exp1 env in Evaluate exp2 env
    | AST.Let(id, exp1, exp2) -> 
        let forwardedExpr = Evaluate exp1 env in Evaluate exp2 (Map.add id forwardedExpr env)
    | AST.Lazy(exp) -> AST.LazyWithEnv(AST.Lazy(exp), env)
    | AST.LazyWithEnv(l, e) -> AST.LazyWithEnv(l, e)
    | AST.Force(e') -> let evalE' = Evaluate e' env in match evalE' with
                                                       | AST.Lazy(exp) -> Evaluate exp env
                                                       | AST.LazyWithEnv(AST.Lazy(exp), env') -> Evaluate exp (mergeMaps env env')
                                                       | _ -> AST.Void(printfn "radi internal error: Trying to force something that is not lazy expression")
    | AST.Chain(l, final) -> List.map (fun e -> Evaluate e env) l /> Evaluate final env
    // Operators:
    | AST.UnaryOperator(op, x) ->
        operator1 op (Evaluate x env)
    | AST.BinaryOperator(op, x, y) ->
        operator2 op (Evaluate x env) (Evaluate y env)
    | AST.Apply(exp1, exp2) ->
        apply (Evaluate exp1 env) (Evaluate exp2 env) env
    | AST.Lambda(id, exp) -> AST.LambdaWithEnv(AST.Lambda(id, exp), env)
    | AST.LambdaWithEnv(a, b) -> AST.LambdaWithEnv(a, b)
    | AST.If (cond, trueExp, falseExp) -> 
        if (unwrapInt (Evaluate cond env)) = 0 then Evaluate falseExp env else Evaluate trueExp env
    // Printing:                            
    | AST.NewLine() -> AST.Void(printfn "")
    | AST.Print (AST.Int(x)) -> AST.Void(printf "%d" x)
    | AST.Print (AST.String(s)) -> AST.Void(printf "%s" s)
    | AST.Print (AST.Void()) -> AST.Void(printf "void")
    | AST.Print (AST.Float(x)) -> AST.Void(printf "%f" x)
    | AST.Print (AST.List(l)) -> AST.Void((printf "[ ") /> unit (List.map (fun x -> Evaluate (AST.Print(x)) env /> printf " ") l) /> printf "]") 
    | AST.Print (exp) -> Evaluate (AST.Print(Evaluate exp env)) env
    // Reading:
    | AST.ReadInt () ->
        try
            let x = System.Console.ReadLine() in AST.Int(int32(x))
        with 
            | :? System.FormatException -> internalError (InputError())
    | AST.ReadString () ->
        AST.String(System.Console.ReadLine())
    | AST.ReadFloat() ->
        try
            let x = System.Console.ReadLine() in AST.Float(float(x))
        with
            | :? System.FormatException -> internalError (InputError())
    // Math functions:
    | AST.MathFunc1 (name, x) ->
        mathFunc1 name (Evaluate x env)
    | AST.MathFunc2 (name, x, y) ->
        mathFunc2 name (Evaluate x env) (Evaluate y env)
    // List functions:
    | AST.ListFunc1 (name, x) -> listFunc1 name (Evaluate x env)
    | AST.ListFunc2 (name, x, y) -> listFunc2 name (Evaluate x env) (Evaluate y env) Evaluate env
    | AST.ListFunc3 (name, x, y, z) -> listFunc3 name (Evaluate x env) (Evaluate y env) (Evaluate z env) Evaluate env
    //
    | AST.UtilFunc1 (name, x) -> utilFunc1 name (Evaluate x env)
and apply exp1 exp2 env =
    match exp1 with
    | AST.LambdaWithEnv(AST.Lambda(id, exp), env1) ->
        Evaluate exp (Map.add id (Evaluate exp2 env) (mergeMaps env env1))
    | x -> Evaluate x env
