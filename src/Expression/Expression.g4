grammar Expression;

options{language=CSharp3;}

@header {
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using maskx.Expression.Expressions;
using ValueType = maskx.Expression.Expressions.ValueType;
}

@members {
private static NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
}

@init {
    numberFormatInfo.NumberDecimalSeparator = ".";
}
run returns[LogicalExpression retValue] 
    : expr EOF { $retValue = $expr.retValue;}
    ;
expr returns[LogicalExpression retValue]
    : first=orExpr '?' middle=expr ':' right=expr   { $retValue = new TernaryExpression($first.retValue, $middle.retValue, $right.retValue);}
    | orExpr                                        { $retValue = $orExpr.retValue;}
    ;

orExpr returns[LogicalExpression retValue] 
    : first=orExpr ('||') andExpr      { $retValue = new BinaryExpression(BinaryExpressionType.Or, $first.retValue, $andExpr.retValue);}
    | andExpr                               { $retValue = $andExpr.retValue;}
    ;

andExpr returns[LogicalExpression retValue] 
    : first=andExpr ('&&') bitOrExpr  { $retValue = new BinaryExpression(BinaryExpressionType.And, $first.retValue, $bitOrExpr.retValue);}
    | bitOrExpr                             { $retValue = $bitOrExpr.retValue;}
    ;

bitOrExpr returns[LogicalExpression retValue] 
    : first=bitOrExpr '|' bitXorExpr    { $retValue = new BinaryExpression(BinaryExpressionType.BitwiseOr, $first.retValue, $bitXorExpr.retValue);}
    | bitXorExpr                        { $retValue = $bitXorExpr.retValue;}
    ;

bitXorExpr returns[LogicalExpression retValue] 
    : first=bitXorExpr '^' bitAndExpr   { $retValue = new BinaryExpression(BinaryExpressionType.BitwiseXOr, $first.retValue, $bitAndExpr.retValue);}
    | bitAndExpr                        { $retValue = $bitAndExpr.retValue;}
    ;

bitAndExpr returns[LogicalExpression retValue] 
    : first=bitAndExpr '&' eqExpr       { $retValue = new BinaryExpression(BinaryExpressionType.BitwiseAnd, $first.retValue, $eqExpr.retValue);}
    | eqExpr                            { $retValue = $eqExpr.retValue;}
    ;

eqExpr returns[LogicalExpression retValue] 
    : first=eqExpr ('=='|'=')  relExpr  { $retValue = new BinaryExpression(BinaryExpressionType.Equal, $first.retValue, $relExpr.retValue);}
    | first=eqExpr ('!='|'<>') relExpr  { $retValue = new BinaryExpression(BinaryExpressionType.NotEqual, $first.retValue, $relExpr.retValue);}
    | relExpr                           { $retValue = $relExpr.retValue;}
    ;

relExpr returns[LogicalExpression retValue] 
    : first=relExpr '<'  shiftExpr      { $retValue = new BinaryExpression(BinaryExpressionType.Lesser, $first.retValue, $shiftExpr.retValue);}
    | first=relExpr '<=' shiftExpr      { $retValue = new BinaryExpression(BinaryExpressionType.LesserOrEqual, $first.retValue, $shiftExpr.retValue);}
    | first=relExpr '>'  shiftExpr      { $retValue = new BinaryExpression(BinaryExpressionType.Greater, $first.retValue, $shiftExpr.retValue);}
    | first=relExpr '>=' shiftExpr      { $retValue = new BinaryExpression(BinaryExpressionType.GreaterOrEqual, $first.retValue, $shiftExpr.retValue);}
    | shiftExpr                         { $retValue = $shiftExpr.retValue;}
    ;

shiftExpr returns[LogicalExpression retValue] 
    : first=shiftExpr '<<' addExpr      { $retValue = new BinaryExpression(BinaryExpressionType.LeftShift, $first.retValue, $addExpr.retValue);}
    | first=shiftExpr '>>' addExpr      { $retValue = new BinaryExpression(BinaryExpressionType.RightShift, $first.retValue, $addExpr.retValue);}
    | addExpr                           { $retValue = $addExpr.retValue;}
    ;

addExpr returns[LogicalExpression retValue] 
    : first=addExpr '+' multExpr        { $retValue = new BinaryExpression(BinaryExpressionType.Plus, $first.retValue, $multExpr.retValue);}
    | first=addExpr '-' multExpr        { $retValue = new BinaryExpression(BinaryExpressionType.Minus, $first.retValue, $multExpr.retValue);}
    | multExpr                          { $retValue = $multExpr.retValue;}
    ;                                   

multExpr returns[LogicalExpression retValue] 
    : first=multExpr '*' unaryExpr      { $retValue = new BinaryExpression(BinaryExpressionType.Times, $first.retValue, $unaryExpr.retValue);}
    | first=multExpr '/' unaryExpr      { $retValue = new BinaryExpression(BinaryExpressionType.Div, $first.retValue, $unaryExpr.retValue);}
    | first=multExpr '%' unaryExpr      { $retValue = new BinaryExpression(BinaryExpressionType.Modulo, $first.retValue, $unaryExpr.retValue);}
    | unaryExpr                         { $retValue = $unaryExpr.retValue;}
    ;
    
unaryExpr returns[LogicalExpression retValue] 
    : ('!' | 'not') primaryExpr         { $retValue = new UnaryExpression(UnaryExpressionType.Not, $primaryExpr.retValue);}
    | '~' primaryExpr                   { $retValue = new UnaryExpression(UnaryExpressionType.BitwiseNot, $primaryExpr.retValue);}
    | '-' primaryExpr                   { $retValue = new UnaryExpression(UnaryExpressionType.Negate, $primaryExpr.retValue);}
    | primaryExpr                       { $retValue = $primaryExpr.retValue;}
    | memberExpr                         { $retValue = $memberExpr.retValue;}
    ;
memberExpr returns[LogicalExpression retValue]
    : left=primaryExpr '.' right=primaryExpr     { $retValue = new MemberExpression($left.retValue,$right.retValue); }   
    | member=memberExpr '.' right=primaryExpr     { $retValue = new MemberExpression($member.retValue,$right.retValue); }   
    ;

primaryExpr returns[LogicalExpression retValue] 
    @init { var args = new List<LogicalExpression>(); }
    : '(' expr ')'                      { $retValue = $expr.retValue;}
    | value                             { $retValue = $value.retValue;}
    | id 
        '(' expr                        { args.Add($expr.retValue);} 
            (',' expr                   { args.Add($expr.retValue);}
            )*
        ')'                             { $retValue = new FunctionExpression((IdentifierExpression)$id.retValue, args.ToArray());}
    | id                                { $retValue = $id.retValue;}
    | id '(' ')'                         { $retValue = new FunctionExpression((IdentifierExpression)$id.retValue, args.ToArray());}
    ;

value returns[LogicalExpression retValue]
    : INTEGER     { try{ $retValue = new ValueExpression(int.Parse($INTEGER.text), ValueType.Integer);} catch { $retValue = new ValueExpression(long.Parse($INTEGER.text), ValueType.Integer); } }
    | FLOAT       { $retValue = new ValueExpression(double.Parse($FLOAT.text, NumberStyles.Float, numberFormatInfo), ValueType.Float);}
    ;

id returns[LogicalExpression retValue]
    : NAME { $retValue = new IdentifierExpression($NAME.text); }
    ;


NAME    : LETTER (LETTER | DIGIT)* ;
INTEGER : DIGIT+  ;
E       : ('E'|'e') ('+'|'-')? DIGIT+ ;

FLOAT
    : DIGIT* '.' DIGIT+ E?
    | DIGIT+ E
    ;


fragment LETTER
    : 'a'..'z'
    | 'A'..'Z'
    | '_'
    ;

fragment DIGIT : '0'..'9';

fragment HexDigit : ('0'..'9'|'a'..'f'|'A'..'F');

/* Ignore white spaces */	
WS : (' '|'\r'|'\t'|'\u000C'|'\n') -> skip;