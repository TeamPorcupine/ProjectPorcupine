#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections;
using UnityEngine;

namespace ProjectPorcupine.Localization
{
    /// <summary>
    /// Holds information about a language's configuration data.
    /// </summary>
    public class LocalizationData
    {
        public readonly string LocalizationCode;

        public bool IsRightToLeft;

        // Even for RTL languages, this is kept as defined in xml. The property does the character reversal
        private string localName;

        public LocalizationData(string localizationCode, string localName, bool isRightToLeft = false)
        {
            this.LocalizationCode = localizationCode;
            this.localName = localName ?? localizationCode;
            this.IsRightToLeft = isRightToLeft;
        }

        public string LocalName
        {
            get
            {
                if (IsRightToLeft == false)
                {
                    return localName;
                }
                else
                {
                    return LocalizationTable.ReverseString(localName);
                }
            }

            set
            {
                localName = value;
            }
        }
    }
}
