module AST

type ID = string
type Expression =
    // Atomics:
    | Int of int                                                                                     
    | Float of float                                                                                 
    | String of string // Not implemented yet...                                                     
    | Void of unit                                                                                   
    | Var of ID
    // Constructions:
    | Let of ID*Expression*Expression                                                                
    | UnaryOperator of string*Expression // Operator + expression                                    
    | BinaryOperator of string*Expression*Expression                                                 
    | Apply of Expression*Expression
    | Lambda of ID*Expression
    // Util expression. Isn't parsed by parser.
    | LambdaWithEnv of Expression*Env // First expression is lambda
    | If of Expression*Expression*Expression // exp1 - condition, exp2 - if true, exp3 - if false  
    | Lazy of Expression
    // Util expression.
    | LazyWithEnv of Expression*Env
    | Force of Expression
    | Chain of Expression list*Expression // chain <exp> $ <exp> $ ... $ <exp> -> <finalExp>
    // I/O:
    | Print of Expression
    | NewLine of unit
    | ReadInt of unit
    | ReadString of unit
    | ReadFloat of unit
    // Math lib
    | MathFunc1 of ID*Expression
    | MathFunc2 of ID*Expression*Expression
    // List:
    | List of Expression list
    | ListFunc1 of ID*Expression
    | ListFunc2 of ID*Expression*Expression
    | ListFunc3 of ID*Expression*Expression*Expression
    // Util
    | UtilFunc1 of ID*Expression
and
    Env = Map<ID, Expression>
