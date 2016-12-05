#region License
// Copyright 2008 Google Inc. All Rights Reserved.
// Author: fraser@google.com (Neil Fraser)
// Author: anteru@developer.shelter13.net (Matthaeus G. Chajdas)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Diff Match and Patch
// http://code.google.com/p/google-diff-match-patch/
//
// Modified and stripped for use in Project Porcupine
#endregion

namespace DiffMatchPatch
{
    public class Diff
    {
        // One of: INSERT, DELETE or EQUAL.
        public Operation operation;

        // The text associated with this diff operation.
        public string text;

        /**
        * Constructor.  Initializes the diff with the provided values.
        * @param operation One of INSERT, DELETE or EQUAL.
        * @param text The text being applied.
        */
        public Diff(Operation operation, string text)
        {
            // Construct a diff with the specified operation and text.
            this.operation = operation;
            this.text = text;
        }

        /**
     * Display a human-readable version of this Diff.
     * @return text version.
     */
        public override string ToString()
        {
            string prettyText = this.text.Replace('\n', '\u00b6');
            return "Diff(" + this.operation + ",\"" + prettyText + "\")";
        }

        /**
     * Is this Diff equivalent to another Diff?
     * @param d Another Diff to compare against.
     * @return true or false.
     */
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Diff return false.
            Diff p = obj as Diff;
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match.
            return p.operation == this.operation && p.text == this.text;
        }

        public bool Equals(Diff obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // Return true if the fields match.
            return obj.operation == this.operation && obj.text == this.text;
        }

        public override int GetHashCode()
        {
            return text.GetHashCode() ^ operation.GetHashCode();
        }
    }
}