/*
LabelChecker - A tool for checking and correcting image labels
Copyright (C) 2025 Tim Johannes Wim Walles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using ImGuiNET;
using Microsoft.Xna.Framework;

namespace LabelChecker.Controls
{
    public abstract class Control(Application app)
    {
        public abstract string Name { get; }
        public Application App { get; set; } = app;

        public virtual void Update(GameTime time)
        {

        }

        public virtual void Draw(ImGuiIOPtr iOPtr)
        {

        }

        public virtual void Cleanup()
        {
            // Base cleanup method that derived classes can override
        }
    }
}