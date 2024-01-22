﻿// THIS FILE IS PART OF ExCSS PROJECT
// THE ExCSS PROJECT IS AN OPENSOURCE LIBRARY LICENSED UNDER THE MIT License.
// COPYRIGHT (C) TylerBrinks. ALL RIGHTS RESERVED.
// GITHUB: https://github.com/TylerBrinks/ExCSS

namespace AntdUI.Svg.ExCSS.Model.TextBlocks
{
    internal class SpecialCharacter : CharacterBlock
    {
        internal static readonly SpecialCharacter Colon = new SpecialCharacter(Specification.Colon, GrammarSegment.Colon);
        internal static readonly SpecialCharacter Comma = new SpecialCharacter(Specification.Comma, GrammarSegment.Comma);
        internal static readonly SpecialCharacter Semicolon = new SpecialCharacter(Specification.Simicolon, GrammarSegment.Semicolon);
        internal static readonly SpecialCharacter Whitespace = new SpecialCharacter(Specification.Space, GrammarSegment.Whitespace);

        SpecialCharacter(char specialCharacter, GrammarSegment type) : base(specialCharacter)
        {
            GrammarSegment = type;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
