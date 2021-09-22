using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;

namespace UIDrawer
{
    public static partial class Drawer
    {
        public static void Draw<T>(this T instance) where T : class, IDrawable, new() => DrawHelper.Draw(instance);
        public static void Draw<T>(this T instance, int unique) where T : class, IDrawable, new() => DrawHelper.Draw(instance, unique);
        public static bool Draw(this ref Vector2 vec2) => DrawVector(ref vec2);
        public static bool Draw(this ref Vector3 vec3) => DrawVector(ref vec3);
        public static bool Draw(this ref Vector4 vec4) => DrawVector(ref vec4);
        public static bool Draw(this ref Color color) => DrawColor(ref color);
        public static bool Draw(this ref int value, string Label) => DrawIntField(ref value, Label);
        public static bool Draw(this ref float value, string Label) => DrawFloatField(ref value, Label);
        public static bool Draw(ref float[] value, string[] Labels) => DrawFloatMultiField(ref value, Labels);
        public static bool DrawField(ref string value, string Label) => DrawTextField(ref value, Label);
        public static bool DrawArea(ref string value, string Label) => DrawTextArea(ref value, Label);
    }
}
