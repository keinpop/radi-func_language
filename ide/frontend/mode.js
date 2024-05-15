define('ace/mode/radi', [], function(require, exports, module) {

  var oop = require("ace/lib/oop");
  var TextMode = require("ace/mode/text").Mode;
  var Tokenizer = require("ace/tokenizer").Tokenizer;
  var RadiHighlightRules = require("ace/mode/radi_highlight_rules").RadiHighlightRules;

  var Mode = function() {
    this.HighlightRules = RadiHighlightRules;
  };
  oop.inherits(Mode, TextMode);

  exports.Mode = Mode;
});

define('ace/mode/radi_highlight_rules', [], function(require, exports, module) {

  var oop = require("ace/lib/oop");
  var TextHighlightRules = require("ace/mode/text_highlight_rules").TextHighlightRules;

  var RadiHighlightRules = function() {

    var keywordMapper = this.createKeywordMapper({
      "variable.language": "this",
      "keyword":
        "Print|ReadInt|ReadFloat|ReadString|app|let|void|lambda|if|else|Square\
        |Sqrt|Sin|Cos|Abs|Pow|lazy|force|chain|NewLine",
      "librarys":
        "io|Math",  
      "constant.language":
        "true|false|null"
    }, "text", true);

    var floatNumber = "(?:(?:(?:(?:(?:(?:\\d+)?(?:\\.\\d+))|(?:(?:\\d+)\\.))|(?:\\d+))(?:[eE][+-]?\\d+))|(?:(?:(?:\\d+)?(?:\\.\\d+))|(?:(?:\\d+)\\.)))";

    this.$rules = {
      "start": [
          {
              token: "string",
              regex: "'.'"
          },
          {
              token: "string",
              regex: '"""',
              next  : [{
                  token : "constant.language.escape",
                  regex : /\\./,
                  next  : "qqstring"
              }, {
                  token : "string",
                  regex : '"""',
                  next  : "start"
              }, {
                  defaultToken: "string"
              }]
          },
          {
              token: "string",
              regex: '"',
              next  : [{
                  token : "constant.language.escape",
                  regex : /\\./,
                  next  : "qqstring"
              }, {
                  token : "string",
                  regex : '"',
                  next  : "start"
              }, {
                  defaultToken: "string"
              }]
          },
          {
              token: ["verbatim.string", "string"],
              regex: '(@?)(")',
              stateName : "qqstring",
              next  : [{
                  token : "constant.language.escape",
                  regex : '""'
              }, {
                  token : "string",
                  regex : '"',
                  next  : "start"
              }, {
                  defaultToken: "string"
              }]
          },
          {
              token: "constant.float",
              regex: "(?:" + floatNumber + "|\\d+)[jJ]\\b"
          },
          {
              token: "constant.float",
              regex: floatNumber
          },
          {
              token: "constant.integer",
              regex: "(?:(?:(?:[1-9]\\d*)|(?:0))|(?:0[oO]?[0-7]+)|(?:0[xX][\\dA-Fa-f]+)|(?:0[bB][01]+))\\b"
          },
          {
              token: ["keyword.type", "variable"],
              regex: "(type\\s)([a-zA-Z0-9_$\-]*\\b)"
          },
          {
              token: keywordMapper,
              regex: "[a-zA-Z_$][a-zA-Z0-9_$]*\\b"
          },
          {
              token: "operator",
              regex: "\\+\\.|\\-\\.|\\*\\.|\\/\\.|#|;;|\\+|\\$|\\-|\\*|\\*\\*\\/|\\/\\/|%|<<|>>|&|\\||\\^|~|<|>|<=|=>|==|!=|<>|<-|=|\\(\\*\\)"
          },
          {
              token: "paren.lparen",
              regex: "[[({]"
          },
          {
              token: "paren.rparen",
              regex: "[\\])}]"
          }
      ]
  };
  this.normalizeRules();
  };

  oop.inherits(RadiHighlightRules, TextHighlightRules);

  exports.RadiHighlightRules = RadiHighlightRules;
});