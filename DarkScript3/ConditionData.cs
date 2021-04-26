using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DarkScript3
{
    public class ConditionData
    {
        public static ConditionData ReadText(string input)
        {
            return JsonConvert.DeserializeObject<ConditionData>(input);
        }

        public static ConditionData ReadFile(string path)
        {
            string input = File.ReadAllText(path);
            return ReadText(input);
        }

        public static ConditionData ReadStream(string resource)
        {
            string input = Resource.Text(resource);
            return ReadText(input);
        }

        /// <summary>
        /// All supported games by name.
        /// </summary>
        [JsonProperty(PropertyName = "games")]
        public List<ConditionGame> Games { get; private set; }

        /// <summary>
        /// Control flow commands (&lt;2000 range, exlcuding labels in bank 1014) which can be ignored for control flow analysis.
        /// 
        /// If any such commands are not in this list and not in the conditions list, it is not safe to rewrite the event.
        /// </summary>
        [JsonProperty(PropertyName = "no_control_flow")]
        public List<string> NoControlFlow { get; private set; }

        /// <summary>
        /// All conditions by name.
        /// </summary>
        [JsonProperty(PropertyName = "conditions")]
        public List<ConditionDoc> Conditions { get; private set; }

        /// <summary>
        /// Registers a game as valid to use with condition data, also basic info if needed.
        /// </summary>
        public class ConditionGame
        {
            /// <summary>
            /// The name of the game. This should match a prefix of a resource file.
            /// </summary>
            [JsonProperty(PropertyName = "name")]
            public string Name { get; private set; }
        }

        /// <summary>
        /// Represents an expression which can be used in a condition.
        /// </summary>
        public class ConditionDoc
        {
            /// <summary>
            /// The basic name of the function. Null only for unconditional variants.
            /// </summary>
            [JsonProperty(PropertyName = "name")]
            public string Name { get; private set; }

            /// <summary>
            /// An imprecise high-level category for this condition, used for condition variable naming heuristics.
            /// </summary>
            [JsonProperty(PropertyName = "category")]
            public string Category { get; private set; }

            /// <summary>
            /// If the same commands have different meanings in different games, the games this expression applies to.
            /// </summary>
            [JsonProperty(PropertyName = "games")]
            public List<string> Games { get; private set; }

            /// <summary>
            /// The command id for the condition group variant of the command, if one exists.
            /// </summary>
            [JsonProperty(PropertyName = "cond")]
            public string Cond { get; private set; }

            /// <summary>
            /// The command id for the skip variant of the command, if one exists.
            /// </summary>
            [JsonProperty(PropertyName = "skip")]
            public string Skip { get; private set; }

            /// <summary>
            /// The command id for the end variant of the command, if one exists.
            /// </summary>
            [JsonProperty(PropertyName = "end")]
            public string End { get; private set; }

            /// <summary>
            /// The command id for the goto variant of the command, if one exists.
            /// </summary>
            [JsonProperty(PropertyName = "goto")]
            public string Goto { get; private set; }

            /// <summary>
            /// The command id for the wait variant of the command, if one exists.
            /// 
            /// For the most part this does not need to be specified, because wait commands don't affect control flow.
            /// </summary>
            [JsonProperty(PropertyName = "wait")]
            public string Wait { get; private set; }

            /// <summary>
            /// If the command can be negated in-place, the field to negate.
            /// 
            /// The negated value is implied to be the first Comparison Type if as_compare is defined.
            /// 
            /// Otherwise, one of the as_bools will be used, with this being trivial for two-value enums.
            /// 
            /// Multiple names are separated using |, for commands with slightly different field names which basically do the same thing.
            /// </summary>
            [JsonProperty(PropertyName = "negate_field")]
            public string NegateField { get; private set; }

            /// <summary>
            /// A mechanic for multiple control flow commands to be compatible if they differ slightly in arguments.
            /// 
            /// The fields must be present for the cond variant of the command. It only applies if they are the last arguments.
            /// 
            /// Otherwise, they can be elided, with the emedf default used instead.
            /// </summary>
            [JsonProperty(PropertyName = "opt_fields")]
            public List<string> OptFields { get; private set; }

            /// <summary>
            /// An alternate name to use, to use the condition like a boolean.
            /// </summary>
            [JsonProperty(PropertyName = "as_bool")]
            public BoolVersion Bool { get; private set; }

            /// <summary>
            /// Multiple alternate names to use, instead of as_bool, if multiple can apply.
            /// </summary>
            [JsonProperty(PropertyName = "as_bools")]
            public List<BoolVersion> Bools { get; private set; }

            /// <summary>
            /// All ways to use the condition like a boolean, collected from as_bool or as_bools.
            /// </summary>
            public List<BoolVersion> AllBools => Bool != null ? new List<BoolVersion> { Bool } : (Bools != null ? Bools : new List<BoolVersion>());

            /// <summary>
            /// An alternate name to use, to use the condition in an integer comparison.
            /// </summary>
            [JsonProperty(PropertyName = "as_compare")]
            public CompareVersion Compare { get; private set; }

            /// <summary>
            /// Whether source code is allowed to contain this constant.
            /// </summary>
            [JsonProperty(PropertyName = "hidden")]
            public bool Hidden { get; private set; }
        }

        /// <summary>
        /// A shorter name for a condition which can be used like Cond() or !Cond().
        /// 
        /// This is based on the value of the field named negate_field.
        /// </summary>
        public class BoolVersion
        {
            /// <summary>
            /// The name of the function. Required.
            /// </summary>
            [JsonProperty(PropertyName = "shortname")]
            public string Name { get; private set; }

            /// <summary>
            /// Which member of the negate_field enum should be considered the true one.
            /// 
            /// This is not needed if the field is a bool.
            /// </summary>
            [JsonProperty(PropertyName = "true")]
            public string True { get; private set; }

            /// <summary>
            /// Which member of the negate_field enum should be considered the false one.
            /// 
            /// This is not needed if the enum only has two values.
            /// </summary>
            [JsonProperty(PropertyName = "false")]
            public string False { get; private set; }

            /// <summary>
            /// Other field values which should be preset for this version to be selected..
            /// 
            /// Currently, fields must have one specific value, and cannot be a choice of multiple.
            /// </summary>
            [JsonProperty(PropertyName = "required")]
            public List<FieldValue> Required { get; private set; }
        }

        /// <summary>
        /// A shorter name for a condition which can be used like Cond() >= 42.
        /// 
        /// The comparison used is based on the value of the first field named Comparison Type, which must be a Comparison Type enum.
        /// </summary>
        public class CompareVersion
        {
            /// <summary>
            /// The name of the function, if not the main comparison op.
            /// </summary>
            [JsonProperty(PropertyName = "shortname")]
            public string Name { get; private set; }

            /// <summary>
            /// For the main comparison op only, the field name used for the left side number.
            /// </summary>
            [JsonProperty(PropertyName = "lhs")]
            public string Lhs { get; private set; }

            /// <summary>
            /// The field name used for the right side number.
            /// </summary>
            [JsonProperty(PropertyName = "rhs")]
            public string Rhs { get; private set; }
        }

        /// <summary>
        /// A specific field enum value to require in selecting a name.
        /// </summary>
        public class FieldValue
        {
            /// <summary>
            /// The name of the field.
            /// </summary>
            [JsonProperty(PropertyName = "field")]
            public string Field { get; private set; }

            /// <summary>
            /// The enum value.
            /// </summary>
            [JsonProperty(PropertyName = "value")]
            public string Value { get; private set; }
        }
    }
}
