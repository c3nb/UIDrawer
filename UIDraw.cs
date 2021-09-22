
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace UIDrawer
{
    /// <summary>
    /// [0.18.0]
    /// </summary>
    public enum DrawType { Auto, Ignore, Field, Slider, Toggle, ToggleGroup, /*MultiToggle, */PopupList, KeyBinding };

    /// <summary>
    /// [0.18.0]
    /// </summary>
    [Flags]
    public enum DrawFieldMask { Any = 0, Public = 1, Serialized = 2, SkipNotSerialized = 4, OnlyDrawAttr = 8 };

    /// <summary>
    /// Provides the Draw method for rendering fields. [0.18.0]
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// Called when values change. For sliders it is called too often.
        /// </summary>
        void OnChange();
    }

    /// <summary>
    /// Specifies which fields to render. [0.18.0]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field, AllowMultiple = false)]
    public class DrawFieldsAttribute : Attribute
    {
        public DrawFieldMask Mask;

        public DrawFieldsAttribute(DrawFieldMask Mask)
        {
            this.Mask = Mask;
        }
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DrawAttribute : Attribute
    {
        public DrawType Type = DrawType.Auto;
        public string Label;
        public int Width = 0;
        public int Height = 0;
        public double Min = double.MinValue;
        public double Max = double.MaxValue;
        /// <summary>
        /// Rounds a double-precision floating-point value to a specified number of fractional digits, and rounds midpoint values to the nearest even number. 
        /// Default 2
        /// </summary>
        public int Precision = 2;
        /// <summary>
        /// Maximum text length.
        /// </summary>
        public int MaxLength = int.MaxValue;
        /// <summary>
        /// Becomes visible if a field value matches. Use format "FieldName|Value". Supports only string, primitive and enum types.
        /// </summary>
        public string VisibleOn;
        /// <summary>
        /// Becomes invisible if a field value matches. Use format "FieldName|Value". Supports only string, primitive and enum types.
        /// </summary>
        public string InvisibleOn;
        /// <summary>
        /// Applies box style.
        /// </summary>
        public bool Box;
        public bool Collapsible;
        public bool Vertical;

        public DrawAttribute()
        {
        }

        public DrawAttribute(string Label)
        {
            this.Label = Label;
        }

        public DrawAttribute(string Label, DrawType Type)
        {
            this.Label = Label;
            this.Type = Type;
        }

        public DrawAttribute(DrawType Type)
        {
            this.Type = Type;
        }
    }

    /// <summary>
    /// [0.22.14]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field, AllowMultiple = false)]
    public class HorizontalAttribute : Attribute
    {
    }

    /// <summary>
    /// Sets options for rendering. [0.19.0]
    /// </summary>
    public struct DrawOptions
    {
        public static implicit operator DrawOptions(string Label) => new DrawOptions(Label);
        public DrawType Type;
        public string Label;
        public int Width;
        public int Height;
        public double Min;
        public double Max;
        /// <summary>
        /// Rounds a double-precision floating-point value to a specified number of fractional digits, and rounds midpoint values to the nearest even number. 
        /// Default 2
        /// </summary>
        public int Precision;
        /// <summary>
        /// Maximum text length.
        /// </summary>
        public int MaxLength;
        /// <summary>
        /// Becomes visible if a field value matches. Use format "FieldName|Value". Supports only string, primitive and enum types.
        /// </summary>
        public string VisibleOn;
        /// <summary>
        /// Becomes invisible if a field value matches. Use format "FieldName|Value". Supports only string, primitive and enum types.
        /// </summary>
        public string InvisibleOn;
        /// <summary>
        /// Applies box style.
        /// </summary>
        public bool Box;
        public bool Collapsible;
        public bool Vertical;
        public bool Horizontal;

        public DrawOptions(string Label)
        {
            this.Label = Label;
            Type = DrawType.Auto;
            Width = 0;
            Height = 0;
            Min = double.MinValue;
            Max = double.MaxValue;
            Precision = 2;
            MaxLength = int.MaxValue;
            VisibleOn = null;
            InvisibleOn = null;
            Box = false;
            Collapsible = false;
            Vertical = false;
            Horizontal = false;
        }

        public DrawOptions(string Label, DrawType Type)
        {
            this.Label = Label;
            this.Type = Type;
            Width = 0;
            Height = 0;
            Min = double.MinValue;
            Max = double.MaxValue;
            Precision = 2;
            MaxLength = int.MaxValue;
            VisibleOn = null;
            InvisibleOn = null;
            Box = false;
            Collapsible = false;
            Vertical = false;
            Horizontal = false;
        }
    }
    internal static class Textures
    {
        internal static Texture2D Window = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
        static Textures()
        {
            Window.LoadImage(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAIAAAEACAYAAACZCaebAAAAnElEQVRIS63MtQHDQADAwPdEZmaG/fdJCq2g7qqLvu/7hRBCZOF9X0ILz/MQWrjvm1DHdV3MFs7zJLRwHAehhX3fCS1s20ZoYV1XQgvLshDqmOeZ2cI0TYQWxnEktDAMA6GFvu8JLXRdR2ihbVtCHU3TMFuo65rQQlVVhBbKsiS0UBQFoYU8zwktZFlGqCNNU2YLSZIQWojjmFDCH22GtZAncD8TAAAAAElFTkSuQmCC"));
        }
    }

    public static partial class Drawer
    {
        internal static GUIStyle window = null;
        internal static GUIStyle h1 = null;
        internal static GUIStyle h2 = null;
        internal static GUIStyle bold = null;
        internal static GUIStyle button = null;
        private static GUIStyle settings = null;
        private static GUIStyle status = null;
        private static GUIStyle www = null;
        private static GUIStyle updates = null;
        static Drawer()
        {
            window = new GUIStyle();
            window.name = "umm window";
            window.normal.background = Textures.Window;
            window.normal.background.wrapMode = TextureWrapMode.Repeat;

            h1 = new GUIStyle();
            h1.name = "umm h1";
            h1.normal.textColor = Color.white;
            h1.fontStyle = FontStyle.Bold;
            h1.alignment = TextAnchor.MiddleCenter;

            h2 = new GUIStyle();
            h2.name = "umm h2";
            h2.normal.textColor = new Color(0.6f, 0.91f, 1f);
            h2.fontStyle = FontStyle.Bold;

            bold = new GUIStyle(GUI.skin.label);
            bold.name = "umm bold";
            bold.normal.textColor = Color.white;
            bold.fontStyle = FontStyle.Bold;

            button = new GUIStyle(GUI.skin.button);
            button.name = "umm button";

            settings = new GUIStyle();
            settings.alignment = TextAnchor.MiddleCenter;
            settings.stretchHeight = true;

            status = new GUIStyle();
            status.alignment = TextAnchor.MiddleCenter;
            status.stretchHeight = true;

            www = new GUIStyle();
            www.alignment = TextAnchor.MiddleCenter;
            www.stretchHeight = true;

            updates = new GUIStyle();
            updates.alignment = TextAnchor.MiddleCenter;
            updates.stretchHeight = true;
        }
        internal static int Scale(int value)
        {
            return value;
        }
        static Type[] fieldTypes = new[] { typeof(int), typeof(long), typeof(float), typeof(double), typeof(int[]), typeof(long[]), typeof(float[]), typeof(double[]),
                typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Color), typeof(string)};
        static Type[] sliderTypes = new[] { typeof(int), typeof(long), typeof(float), typeof(double) };
        static Type[] toggleTypes = new[] { typeof(bool) };
        static Type[] specialTypes = new[] { typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Color) };
        static float drawHeight = 22;

        /// <summary>
        /// [0.18.0]
        /// </summary>
        /// <returns>
        /// Returns true if the value has changed.
        /// </returns>
        internal static bool DrawVector(ref Vector2 vec, GUIStyle style = null, params GUILayoutOption[] option)
        {
            var values = new float[2] { vec.x, vec.y };
            var labels = new string[2] { "x", "y" };
            if (DrawFloatMultiField(ref values, labels, style, option))
            {
                vec = new Vector2(values[0], values[1]);
                return true;
            }
            return false;
        }

        /// <summary>
        /// [0.18.0]
        /// </summary>
        internal static void DrawVector(Vector2 vec, Action<Vector2> onChange, GUIStyle style = null, params GUILayoutOption[] option)
        {
            if (onChange == null)
            {
                throw new ArgumentNullException("onChange");
            }
            if (DrawVector(ref vec, style, option))
            {
                onChange(vec);
            }
        }

        /// <summary>
        /// [0.18.0]
        /// </summary>
        /// <returns>
        /// Returns true if the value has changed.
        /// </returns>
        internal static bool DrawVector(ref Vector3 vec, GUIStyle style = null, params GUILayoutOption[] option)
        {
            var values = new float[3] { vec.x, vec.y, vec.z };
            var labels = new string[3] { "x", "y", "z" };
            if (DrawFloatMultiField(ref values, labels, style, option))
            {
                vec = new Vector3(values[0], values[1], values[2]);
                return true;
            }
            return false;
        }

        /// <summary>
        /// [0.18.0]
        /// </summary>
        internal static void DrawVector(Vector3 vec, Action<Vector3> onChange, GUIStyle style = null, params GUILayoutOption[] option)
        {
            if (onChange == null)
            {
                throw new ArgumentNullException("onChange");
            }
            if (DrawVector(ref vec, style, option))
            {
                onChange(vec);
            }
        }

        /// <summary>
        /// [0.18.0]
        /// </summary>
        /// <returns>
        /// Returns true if the value has changed.
        /// </returns>
        internal static bool DrawVector(ref Vector4 vec, GUIStyle style = null, params GUILayoutOption[] option)
        {
            var values = new float[4] { vec.x, vec.y, vec.z, vec.w };
            var labels = new string[4] { "x", "y", "z", "w" };
            if (DrawFloatMultiField(ref values, labels, style, option))
            {
                vec = new Vector4(values[0], values[1], values[2], values[3]);
                return true;
            }
            return false;
        }

        /// <summary>
        /// [0.18.0]
        /// </summary>
        internal static void DrawVector(Vector4 vec, Action<Vector4> onChange, GUIStyle style = null, params GUILayoutOption[] option)
        {
            if (onChange == null)
            {
                throw new ArgumentNullException("onChange");
            }
            if (DrawVector(ref vec, style, option))
            {
                onChange(vec);
            }
        }

        /// <summary>
        /// [0.18.0]
        /// </summary>
        /// <returns>
        /// Returns true if the value has changed.
        /// </returns>
        internal static bool DrawColor(ref Color vec, GUIStyle style = null, params GUILayoutOption[] option)
        {
            var values = new float[4] { vec.r, vec.g, vec.b, vec.a };
            var labels = new string[4] { "r", "g", "b", "a" };
            if (DrawFloatMultiField(ref values, labels, style, option))
            {
                vec = new Color(values[0], values[1], values[2], values[3]);
                return true;
            }
            return false;
        }

        /// <summary>
        /// [0.18.0]
        /// </summary>
        internal static void DrawColor(Color vec, Action<Color> onChange, GUIStyle style = null, params GUILayoutOption[] option)
        {
            if (onChange == null)
            {
                throw new ArgumentNullException("onChange");
            }
            if (DrawColor(ref vec, style, option))
            {
                onChange(vec);
            }
        }

        /// <summary>
        /// [0.18.0]
        /// </summary>
        /// <returns>
        /// Returns true if the value has changed.
        /// </returns>
        internal static bool DrawFloatMultiField(ref float[] values, string[] labels, GUIStyle style = null, params GUILayoutOption[] option)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentNullException(nameof(values));
            if (labels == null || labels.Length == 0)
                throw new ArgumentNullException(nameof(labels));
            if (values.Length != labels.Length)
                throw new ArgumentOutOfRangeException(nameof(labels));

            var changed = false;
            var result = new float[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(labels[i], GUILayout.ExpandWidth(false));
                var str = GUILayout.TextField(values[i].ToString("f6"), style ?? GUI.skin.textField, option);
                GUILayout.EndHorizontal();
                if (string.IsNullOrEmpty(str))
                {
                    result[i] = 0;
                }
                else
                {
                    if (float.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.CurrentInfo, out var num))
                    {
                        result[i] = num;
                    }
                    else
                    {
                        result[i] = 0;
                    }
                }
                if (result[i] != values[i])
                {
                    changed = true;
                }
            }

            values = result;
            return changed;
        }

        /// <summary>
        /// [0.19.0]
        /// </summary>
        /// <returns>
        /// Returns true if the value has changed.
        /// </returns>
        internal static bool DrawFloatField(ref float value, string label, GUIStyle style = null, params GUILayoutOption[] option)
        {
            var old = value;
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            var str = GUILayout.TextField(value.ToString("f6"), style ?? GUI.skin.textField, option);
            if (string.IsNullOrEmpty(str))
            {
                value = 0;
            }
            else
            {
                if (float.TryParse(str, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo, out var num))
                {
                    value = num;
                }
                else
                {
                    value = 0;
                }
            }
            return value != old;
        }
        internal static bool DrawTextField(ref string value, string label, GUIStyle style = null, params GUILayoutOption[] option)
        {
            var old = value;
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            value = GUILayout.TextField(value, style ?? GUI.skin.textField, option);
            return value != old;
        }
        internal static bool DrawTextArea(ref string value, string label, GUIStyle style = null, params GUILayoutOption[] option)
        {
            var old = value;
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            value = GUILayout.TextArea(value, style ?? GUI.skin.textArea, option);
            return value != old;
        }
        /// <summary>
        /// [0.19.0]
        /// </summary>
        internal static void DrawFloatField(float value, string label, Action<float> onChange, GUIStyle style = null, params GUILayoutOption[] option)
        {
            if (onChange == null)
            {
                throw new ArgumentNullException("onChange");
            }
            if (DrawFloatField(ref value, label, style, option))
            {
                onChange(value);
            }
        }

        /// <summary>
        /// [0.19.0]
        /// </summary>
        /// <returns>
        /// Returns true if the value has changed.
        /// </returns>
        internal static bool DrawIntField(ref int value, string label, GUIStyle style = null, params GUILayoutOption[] option)
        {
            var old = value;
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            var str = GUILayout.TextField(value.ToString(), style ?? GUI.skin.textField, option);
            if (string.IsNullOrEmpty(str))
            {
                value = 0;
            }
            else
            {
                if (int.TryParse(str, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.CurrentInfo, out var num))
                {
                    value = num;
                }
                else
                {
                    value = 0;
                }
            }
            return value != old;
        }

        /// <summary>
        /// [0.19.0]
        /// </summary>
        internal static void DrawIntField(int value, string label, Action<int> onChange, GUIStyle style = null, params GUILayoutOption[] option)
        {
            if (onChange == null)
            {
                throw new ArgumentNullException("onChange");
            }
            if (DrawIntField(ref value, label, style, option))
            {
                onChange(value);
            }
        }

        private static List<int> collapsibleStates = new List<int>();

        private static bool DependsOn(string str, object container, Type t = null)
        {
            Type type = t ?? container.GetType();
            var param = str.Split('|');
            if (param.Length != 2)
            {
                throw new Exception($"VisibleOn/InvisibleOn({str}) must have 2 params, name and value, e.g (FieldName|True).");
            }
            var dependsOnField = type.GetField(param[0], System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dependsOnField == null)
            {
                throw new Exception($"Field '{param[0]}' not found.");
            }
            if (!dependsOnField.FieldType.IsPrimitive && !dependsOnField.FieldType.IsEnum)
            {
                throw new Exception($"Type '{dependsOnField.FieldType.Name}' is not supported.");
            }
            object dependsOnValue = null;
            if (dependsOnField.FieldType.IsEnum)
            {
                try
                {
                    dependsOnValue = Enum.Parse(dependsOnField.FieldType, param[1]);
                    if (dependsOnValue == null)
                    {
                        throw new Exception($"Value '{param[1]}' cannot be parsed.");
                    }
                }
                catch (Exception e)
                {
                    //mod.Logger.Log($"Parse value VisibleOn/InvisibleOn({str})");
                    throw e;
                }
            }
            else if (dependsOnField.FieldType == typeof(string))
            {
                dependsOnValue = param[1];
            }
            else
            {
                try
                {
                    dependsOnValue = Convert.ChangeType(param[1], dependsOnField.FieldType);
                    if (dependsOnValue == null)
                    {
                        throw new Exception($"Value '{param[1]}' cannot be parsed.");
                    }
                }
                catch (Exception e)
                {
                    //mod.Logger.Log($"Parse value VisibleOn/InvisibleOn({str})");
                    throw e;
                }
            }

            var value = dependsOnField.GetValue(container);
            return value.GetHashCode() == dependsOnValue.GetHashCode();
        }

        internal static bool Draw(object container, Type type, DrawFieldMask defaultMask, int unique)
        {
            bool changed = false;
            var options = new List<GUILayoutOption>();
            DrawFieldMask mask = defaultMask;
            foreach (DrawFieldsAttribute attr in type.GetCustomAttributes(typeof(DrawFieldsAttribute), false))
            {
                mask = attr.Mask;
            }
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var f in fields)
            {
                DrawAttribute a = new DrawAttribute();
                var attributes = f.GetCustomAttributes(typeof(DrawAttribute), false);
                if (attributes.Length > 0)
                {
                    foreach (DrawAttribute a_ in attributes)
                    {
                        a = a_;
                        a.Width = a.Width != 0 ? Scale(a.Width) : 0;
                        a.Height = a.Height != 0 ? Scale(a.Height) : 0;
                    }

                    if (a.Type == DrawType.Ignore)
                        continue;

                    if (!string.IsNullOrEmpty(a.VisibleOn))
                    {
                        if (!DependsOn(a.VisibleOn, container, type))
                        {
                            continue;
                        }
                    }
                    else if (!string.IsNullOrEmpty(a.InvisibleOn))
                    {
                        if (DependsOn(a.InvisibleOn, container, type))
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    if ((mask & DrawFieldMask.OnlyDrawAttr) == 0 && ((mask & DrawFieldMask.SkipNotSerialized) == 0 || !f.IsNotSerialized)
                        && ((mask & DrawFieldMask.Public) > 0 && f.IsPublic
                        || (mask & DrawFieldMask.Serialized) > 0 && f.GetCustomAttributes(typeof(SerializeField), false).Length > 0
                        || (mask & DrawFieldMask.Public) == 0 && (mask & DrawFieldMask.Serialized) == 0))
                    {
                        foreach (RangeAttribute a_ in f.GetCustomAttributes(typeof(RangeAttribute), false))
                        {
                            a.Type = DrawType.Slider;
                            a.Min = a_.min;
                            a.Max = a_.max;
                            break;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                foreach (SpaceAttribute a_ in f.GetCustomAttributes(typeof(SpaceAttribute), false))
                {
                    GUILayout.Space(Scale((int)a_.height));
                }

                foreach (HeaderAttribute a_ in f.GetCustomAttributes(typeof(HeaderAttribute), false))
                {
                    GUILayout.Label(a_.header, bold, GUILayout.ExpandWidth(false));
                }

                var fieldName = a.Label == null ? f.Name : a.Label;

                if ((f.FieldType.IsClass && !f.FieldType.IsArray || f.FieldType.IsValueType && !f.FieldType.IsPrimitive && !f.FieldType.IsEnum) && !Array.Exists(specialTypes, x => x == f.FieldType))
                {
                    defaultMask = mask;
                    foreach (DrawFieldsAttribute attr in f.GetCustomAttributes(typeof(DrawFieldsAttribute), false))
                    {
                        defaultMask = attr.Mask;
                    }

                    var box = a.Box || a.Collapsible && collapsibleStates.Exists(x => x == f.MetadataToken);
                    var horizontal = f.GetCustomAttributes(typeof(HorizontalAttribute), false).Length > 0 || f.FieldType.GetCustomAttributes(typeof(HorizontalAttribute), false).Length > 0;
                    if (horizontal)
                    {
                        GUILayout.BeginHorizontal(box ? "box" : "");
                        box = false;
                    }

                    if (a.Collapsible)
                        GUILayout.BeginHorizontal();

                    if (!string.IsNullOrEmpty(fieldName))
                        GUILayout.Label($"{fieldName}", GUILayout.ExpandWidth(false));

                    var visible = true;
                    if (a.Collapsible)
                    {
                        if (!string.IsNullOrEmpty(fieldName))
                            GUILayout.Space(5);
                        visible = collapsibleStates.Exists(x => x == f.MetadataToken);
                        if (GUILayout.Button(visible ? "Hide" : "Show", GUILayout.ExpandWidth(false)))
                        {
                            if (visible)
                            {
                                collapsibleStates.Remove(f.MetadataToken);
                            }
                            else
                            {
                                collapsibleStates.Add(f.MetadataToken);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    if (visible)
                    {
                        if (box)
                            GUILayout.BeginVertical("box");
                        var val = f.GetValue(container);
                        if (typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType) && val is UnityEngine.Object obj)
                        {
                            GUILayout.Label(obj.name, GUILayout.ExpandWidth(false));
                        }
                        else
                        {
                            if (Draw(val, f.FieldType, defaultMask, f.Name.GetHashCode() + unique))
                            {
                                changed = true;
                                f.SetValue(container, val);
                            }
                        }
                        if (box)
                            GUILayout.EndVertical();
                    }

                    if (horizontal)
                        GUILayout.EndHorizontal();
                    continue;
                }

                options.Clear();
                if (a.Type == DrawType.Auto)
                {
                    if (Array.Exists(fieldTypes, x => x == f.FieldType))
                    {
                        a.Type = DrawType.Field;
                    }
                    else if (Array.Exists(toggleTypes, x => x == f.FieldType))
                    {
                        a.Type = DrawType.Toggle;
                    }
                    else if (f.FieldType.IsEnum)
                    {
                        if (f.GetCustomAttributes(typeof(FlagsAttribute), false).Length == 0)
                            a.Type = DrawType.PopupList;
                    }
                    //else if (f.FieldType == typeof(KeyBinding))
                    //{
                    //    a.Type = DrawType.KeyBinding;
                    //}
                }

                if (a.Type == DrawType.Field)
                {
                    if (!Array.Exists(fieldTypes, x => x == f.FieldType) && !f.FieldType.IsArray)
                    {
                        throw new Exception($"Type {f.FieldType} can't be drawn as {DrawType.Field}");
                    }

                    options.Add(a.Width != 0 ? GUILayout.Width(a.Width) : GUILayout.Width(Scale(100)));
                    options.Add(a.Height != 0 ? GUILayout.Height(a.Height) : GUILayout.Height(Scale((int)drawHeight)));
                    if (f.FieldType == typeof(Vector2))
                    {
                        if (a.Vertical)
                            GUILayout.BeginVertical();
                        else
                            GUILayout.BeginHorizontal();
                        GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                        if (!a.Vertical)
                            GUILayout.Space(Scale(5));
                        var vec = (Vector2)f.GetValue(container);
                        if (DrawVector(ref vec, null, options.ToArray()))
                        {
                            f.SetValue(container, vec);
                            changed = true;
                        }
                        if (a.Vertical)
                        {
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                    }
                    else if (f.FieldType == typeof(Vector3))
                    {
                        if (a.Vertical)
                            GUILayout.BeginVertical();
                        else
                            GUILayout.BeginHorizontal();
                        GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                        if (!a.Vertical)
                            GUILayout.Space(Scale(5));
                        var vec = (Vector3)f.GetValue(container);
                        if (DrawVector(ref vec, null, options.ToArray()))
                        {
                            f.SetValue(container, vec);
                            changed = true;
                        }
                        if (a.Vertical)
                        {
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                    }
                    else if (f.FieldType == typeof(Vector4))
                    {
                        if (a.Vertical)
                            GUILayout.BeginVertical();
                        else
                            GUILayout.BeginHorizontal();
                        GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                        if (!a.Vertical)
                            GUILayout.Space(Scale(5));
                        var vec = (Vector4)f.GetValue(container);
                        if (DrawVector(ref vec, null, options.ToArray()))
                        {
                            f.SetValue(container, vec);
                            changed = true;
                        }
                        if (a.Vertical)
                        {
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                    }
                    else if (f.FieldType == typeof(Color))
                    {
                        if (a.Vertical)
                            GUILayout.BeginVertical();
                        else
                            GUILayout.BeginHorizontal();
                        GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                        if (!a.Vertical)
                            GUILayout.Space(Scale(5));
                        var vec = (Color)f.GetValue(container);
                        if (DrawColor(ref vec, null, options.ToArray()))
                        {
                            f.SetValue(container, vec);
                            changed = true;
                        }
                        if (a.Vertical)
                        {
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        //var val = f.GetValue(container).ToString();
                        var obj = f.GetValue(container);
                        Type elementType = null;
                        object[] values = null;
                        if (f.FieldType.IsArray)
                        {
                            if (obj is IEnumerable array)
                            {
                                values = array.Cast<object>().ToArray();
                                elementType = obj.GetType().GetElementType();
                            }
                        }
                        else
                        {
                            values = new object[] { obj };
                            elementType = obj.GetType();
                        }

                        if (values == null)
                            continue;

                        var _changed = false;

                        a.Vertical = a.Vertical || f.FieldType.IsArray;
                        if (a.Vertical)
                            GUILayout.BeginVertical();
                        else
                            GUILayout.BeginHorizontal();
                        if (f.FieldType.IsArray)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                            GUILayout.Space(Scale(5));
                            if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                            {
                                Array.Resize(ref values, Math.Min(values.Length + 1, int.MaxValue));
                                values[values.Length - 1] = Convert.ChangeType("0", elementType);
                                _changed = true;
                                changed = true;
                            }
                            if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                            {
                                Array.Resize(ref values, Math.Max(values.Length - 1, 0));
                                if (values.Length > 0)
                                    values[values.Length - 1] = Convert.ChangeType("0", elementType);
                                _changed = true;
                                changed = true;
                            }
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                        }
                        if (!a.Vertical)
                            GUILayout.Space(Scale(5));

                        if (values.Length > 0)
                        {
                            var isFloat = f.FieldType == typeof(float) || f.FieldType == typeof(double) || f.FieldType == typeof(float[]) || f.FieldType == typeof(double[]);
                            for (int i = 0; i < values.Length; i++)
                            {
                                var val = values[i].ToString();
                                if (a.Precision >= 0 && isFloat)
                                {
                                    if (Double.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo, out var num))
                                    {
                                        val = num.ToString($"f{a.Precision}");
                                    }
                                }
                                if (f.FieldType.IsArray)
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Label($"  [{i}] ", GUILayout.ExpandWidth(false));
                                }
                                var result = f.FieldType == typeof(string) ? GUILayout.TextField(val, a.MaxLength, options.ToArray()) : GUILayout.TextField(val, options.ToArray());
                                if (f.FieldType.IsArray)
                                {
                                    GUILayout.EndHorizontal();
                                }
                                if (result != val)
                                {
                                    if (string.IsNullOrEmpty(result))
                                    {
                                        if (f.FieldType != typeof(string))
                                            result = "0";
                                    }
                                    else
                                    {
                                        if (Double.TryParse(result, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo, out var num))
                                        {
                                            num = Math.Max(num, a.Min);
                                            num = Math.Min(num, a.Max);
                                            result = num.ToString();
                                        }
                                        else
                                        {
                                            result = "0";
                                        }
                                    }
                                    values[i] = Convert.ChangeType(result, elementType);
                                    changed = true;
                                    _changed = true;
                                }
                            }
                        }
                        if (_changed)
                        {
                            if (f.FieldType.IsArray)
                            {
                                if (elementType == typeof(float))
                                    f.SetValue(container, Array.ConvertAll(values, x => (float)x));
                                else if (elementType == typeof(int))
                                    f.SetValue(container, Array.ConvertAll(values, x => (int)x));
                                else if (elementType == typeof(long))
                                    f.SetValue(container, Array.ConvertAll(values, x => (long)x));
                                else if (elementType == typeof(double))
                                    f.SetValue(container, Array.ConvertAll(values, x => (double)x));
                            }
                            else
                            {
                                f.SetValue(container, values[0]);
                            }
                        }
                        if (a.Vertical)
                            GUILayout.EndVertical();
                        else
                            GUILayout.EndHorizontal();
                    }
                }
                else if (a.Type == DrawType.Slider)
                {
                    if (!Array.Exists(sliderTypes, x => x == f.FieldType))
                    {
                        throw new Exception($"Type {f.FieldType} can't be drawn as {DrawType.Slider}");
                    }

                    options.Add(a.Width != 0 ? GUILayout.Width(a.Width) : GUILayout.Width(Scale(200)));
                    options.Add(a.Height != 0 ? GUILayout.Height(a.Height) : GUILayout.Height(Scale((int)drawHeight)));
                    if (a.Vertical)
                        GUILayout.BeginVertical();
                    else
                        GUILayout.BeginHorizontal();
                    GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                    if (!a.Vertical)
                        GUILayout.Space(Scale(5));
                    var val = f.GetValue(container).ToString();
                    if (!Double.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo, out var num))
                    {
                        num = 0;
                    }
                    if (a.Vertical)
                        GUILayout.BeginHorizontal();
                    var fnum = (float)num;
                    var result = GUILayout.HorizontalSlider(fnum, (float)a.Min, (float)a.Max, options.ToArray());
                    if (!a.Vertical)
                        GUILayout.Space(Scale(5));
                    GUILayout.Label(result.ToString(), GUILayout.ExpandWidth(false), GUILayout.Height(Scale((int)drawHeight)));
                    if (a.Vertical)
                        GUILayout.EndHorizontal();
                    if (a.Vertical)
                        GUILayout.EndVertical();
                    else
                        GUILayout.EndHorizontal();
                    if (result != fnum)
                    {
                        if ((f.FieldType == typeof(float) || f.FieldType == typeof(double)) && a.Precision >= 0)
                            result = (float)Math.Round(result, a.Precision);
                        f.SetValue(container, Convert.ChangeType(result, f.FieldType));
                        changed = true;
                    }
                }
                else if (a.Type == DrawType.Toggle)
                {
                    if (!Array.Exists(toggleTypes, x => x == f.FieldType))
                    {
                        throw new Exception($"Type {f.FieldType} can't be drawn as {DrawType.Toggle}");
                    }

                    options.Add(GUILayout.ExpandWidth(false));
                    options.Add(a.Height != 0 ? GUILayout.Height(a.Height) : GUILayout.Height(Scale((int)drawHeight)));
                    if (a.Vertical)
                        GUILayout.BeginVertical();
                    else
                        GUILayout.BeginHorizontal();
                    GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                    var val = (bool)f.GetValue(container);
                    var result = GUILayout.Toggle(val, "", options.ToArray());
                    if (a.Vertical)
                        GUILayout.EndVertical();
                    else
                        GUILayout.EndHorizontal();
                    if (result != val)
                    {
                        f.SetValue(container, Convert.ChangeType(result, f.FieldType));
                        changed = true;
                    }
                }
                else if (a.Type == DrawType.ToggleGroup)
                {
                    if (!f.FieldType.IsEnum)
                    {
                        throw new Exception($"Type {f.FieldType} can't be drawn as {DrawType.ToggleGroup}");
                    }
                    if (f.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                    {
                        throw new Exception($"Type {f.FieldType}/{DrawType.ToggleGroup} incompatible with Flag attribute.");
                    }

                    options.Add(GUILayout.ExpandWidth(false));
                    options.Add(a.Height != 0 ? GUILayout.Height(a.Height) : GUILayout.Height(Scale((int)drawHeight)));
                    if (a.Vertical)
                        GUILayout.BeginVertical();
                    else
                        GUILayout.BeginHorizontal();
                    GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                    if (!a.Vertical)
                        GUILayout.Space(Scale(5));
                    var values = Enum.GetNames(f.FieldType);
                    var val = (int)f.GetValue(container);

                    if (ToggleGroup(ref val, values, null, options.ToArray()))
                    {
                        var v = Enum.Parse(f.FieldType, values[val]);
                        f.SetValue(container, v);
                        changed = true;
                    }
                    if (a.Vertical)
                        GUILayout.EndVertical();
                    else
                        GUILayout.EndHorizontal();
                }
                else if (a.Type == DrawType.PopupList)
                {
                    if (!f.FieldType.IsEnum)
                    {
                        throw new Exception($"Type {f.FieldType} can't be drawn as {DrawType.PopupList}");
                    }
                    if (f.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                    {
                        throw new Exception($"Type {f.FieldType}/{DrawType.ToggleGroup} incompatible with Flag attribute.");
                    }

                    options.Add(GUILayout.ExpandWidth(false));
                    options.Add(a.Height != 0 ? GUILayout.Height(a.Height) : GUILayout.Height(Scale((int)drawHeight)));
                    if (a.Vertical)
                        GUILayout.BeginVertical();
                    else
                        GUILayout.BeginHorizontal();
                    GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                    if (!a.Vertical)
                        GUILayout.Space(Scale(5));
                    var values = Enum.GetNames(f.FieldType);
                    var val = (int)f.GetValue(container);
                    if (PopupToggleGroup(ref val, values, fieldName, unique, null, options.ToArray()))
                    {
                        var v = Enum.Parse(f.FieldType, values[val]);
                        f.SetValue(container, v);
                        changed = true;
                    }
                    if (a.Vertical)
                        GUILayout.EndVertical();
                    else
                        GUILayout.EndHorizontal();
                }
                //else if (a.Type == DrawType.KeyBinding)
                //{
                //    if (f.FieldType != typeof(KeyBinding))
                //    {
                //        throw new Exception($"Type {f.FieldType} can't be drawn as {DrawType.KeyBinding}");
                //    }

                //    if (a.Vertical)
                //        GUILayout.BeginVertical();
                //    else
                //        GUILayout.BeginHorizontal();
                //    GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                //    if (!a.Vertical)
                //        GUILayout.Space(Scale(5));
                //    var key = (KeyBinding)f.GetValue(container);
                //    if (DrawKeybinding(ref key, fieldName, unique, null, options.ToArray()))
                //    {
                //        f.SetValue(container, key);
                //        changed = true;
                //    }
                //    if (a.Vertical)
                //    {
                //        GUILayout.EndVertical();
                //    }
                //    else
                //    {
                //        GUILayout.FlexibleSpace();
                //        GUILayout.EndHorizontal();
                //    }
                //}
            }
            return changed;
        }
        public static bool Draw(this FieldInfo field, object instance, DrawOptions Options)
        {
            bool changed = false;
            var options = new List<GUILayoutOption>();
            if (Options.Label != null)
            {
                Options.Width = Options.Width != 0 ? Scale(Options.Width) : 0;
                Options.Height = Options.Height != 0 ? Scale(Options.Height) : 0;

                if (Options.Type == DrawType.Ignore)
                    return false;

                if (!string.IsNullOrEmpty(Options.VisibleOn))
                {
                    if (!DependsOn(Options.VisibleOn, instance))
                    {
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(Options.InvisibleOn))
                {
                    if (DependsOn(Options.InvisibleOn, instance))
                    {
                        return false;
                    }
                }
            }

            foreach (SpaceAttribute a_ in field.GetCustomAttributes(typeof(SpaceAttribute), false))
            {
                GUILayout.Space(Scale((int)a_.height));
            }

            foreach (HeaderAttribute a_ in field.GetCustomAttributes(typeof(HeaderAttribute), false))
            {
                GUILayout.Label(a_.header, bold, GUILayout.ExpandWidth(false));
            }

            var fieldName = Options.Label == null ? field.Name : Options.Label;

            if ((field.FieldType.IsClass && !field.FieldType.IsArray || field.FieldType.IsValueType && !field.FieldType.IsPrimitive && !field.FieldType.IsEnum) && !Array.Exists(specialTypes, x => x == field.FieldType))
            {

                var box = Options.Box || Options.Collapsible && collapsibleStates.Exists(x => x == field.MetadataToken);
                if (Options.Horizontal)
                {
                    GUILayout.BeginHorizontal(box ? "box" : "");
                    box = false;
                }

                if (Options.Collapsible)
                    GUILayout.BeginHorizontal();

                if (!string.IsNullOrEmpty(fieldName))
                    GUILayout.Label($"{fieldName}", GUILayout.ExpandWidth(false));

                var visible = true;
                if (Options.Collapsible)
                {
                    if (!string.IsNullOrEmpty(fieldName))
                        GUILayout.Space(5);
                    visible = collapsibleStates.Exists(x => x == field.MetadataToken);
                    if (GUILayout.Button(visible ? "Hide" : "Show", GUILayout.ExpandWidth(false)))
                    {
                        if (visible)
                        {
                            collapsibleStates.Remove(field.MetadataToken);
                        }
                        else
                        {
                            collapsibleStates.Add(field.MetadataToken);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                if (visible)
                {
                    if (box)
                        GUILayout.BeginVertical("box");
                    var val = field.GetValue(instance);
                    if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType) && val is UnityEngine.Object obj)
                    {
                        GUILayout.Label(obj.name, GUILayout.ExpandWidth(false));
                    }
                    else
                    {
                        if (Draw(field, val, Options))
                        {
                            changed = true;
                            field.SetValue(instance, val);
                        }
                    }
                    if (box)
                        GUILayout.EndVertical();
                }

                if (Options.Horizontal)
                    GUILayout.EndHorizontal();
                return false;
            }

            options.Clear();
            if (Options.Type == DrawType.Auto)
            {
                if (Array.Exists(fieldTypes, x => x == field.FieldType))
                {
                    Options.Type = DrawType.Field;
                }
                else if (Array.Exists(toggleTypes, x => x == field.FieldType))
                {
                    Options.Type = DrawType.Toggle;
                }
                else if (field.FieldType.IsEnum)
                {
                    if (field.GetCustomAttributes(typeof(FlagsAttribute), false).Length == 0)
                        Options.Type = DrawType.PopupList;
                }
                //else if (field.FieldType == typeof(KeyBinding))
                //{
                //    Options.Type = DrawType.KeyBinding;
                //}
            }

            if (Options.Type == DrawType.Field)
            {
                if (!Array.Exists(fieldTypes, x => x == field.FieldType) && !field.FieldType.IsArray)
                {
                    throw new Exception($"Type {field.FieldType} can't be drawn as {DrawType.Field}");
                }

                options.Add(Options.Width != 0 ? GUILayout.Width(Options.Width) : GUILayout.Width(Scale(100)));
                options.Add(Options.Height != 0 ? GUILayout.Height(Options.Height) : GUILayout.Height(Scale((int)drawHeight)));
                if (field.FieldType == typeof(Vector2))
                {
                    if (Options.Vertical)
                        GUILayout.BeginVertical();
                    else
                        GUILayout.BeginHorizontal();
                    GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                    if (!Options.Vertical)
                        GUILayout.Space(Scale(5));
                    var vec = (Vector2)field.GetValue(instance);
                    if (DrawVector(ref vec, null, options.ToArray()))
                    {
                        field.SetValue(instance, vec);
                        changed = true;
                    }
                    if (Options.Vertical)
                    {
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
                else if (field.FieldType == typeof(Vector3))
                {
                    if (Options.Vertical)
                        GUILayout.BeginVertical();
                    else
                        GUILayout.BeginHorizontal();
                    GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                    if (!Options.Vertical)
                        GUILayout.Space(Scale(5));
                    var vec = (Vector3)field.GetValue(instance);
                    if (DrawVector(ref vec, null, options.ToArray()))
                    {
                        field.SetValue(instance, vec);
                        changed = true;
                    }
                    if (Options.Vertical)
                    {
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
                else if (field.FieldType == typeof(Vector4))
                {
                    if (Options.Vertical)
                        GUILayout.BeginVertical();
                    else
                        GUILayout.BeginHorizontal();
                    GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                    if (!Options.Vertical)
                        GUILayout.Space(Scale(5));
                    var vec = (Vector4)field.GetValue(instance);
                    if (DrawVector(ref vec, null, options.ToArray()))
                    {
                        field.SetValue(instance, vec);
                        changed = true;
                    }
                    if (Options.Vertical)
                    {
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
                else if (field.FieldType == typeof(Color))
                {
                    if (Options.Vertical)
                        GUILayout.BeginVertical();
                    else
                        GUILayout.BeginHorizontal();
                    GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                    if (!Options.Vertical)
                        GUILayout.Space(Scale(5));
                    var vec = (Color)field.GetValue(instance);
                    if (DrawColor(ref vec, null, options.ToArray()))
                    {
                        field.SetValue(instance, vec);
                        changed = true;
                    }
                    if (Options.Vertical)
                    {
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    //var val = field.GetValue(container).ToString();
                    var obj = field.GetValue(instance);
                    Type elementType = null;
                    object[] values = null;
                    if (field.FieldType.IsArray)
                    {
                        if (obj is IEnumerable array)
                        {
                            values = array.Cast<object>().ToArray();
                            elementType = obj.GetType().GetElementType();
                        }
                    }
                    else
                    {
                        values = new object[] { obj };
                        elementType = obj.GetType();
                    }

                    if (values == null)
                        return false;

                    var _changed = false;

                    Options.Vertical = Options.Vertical || field.FieldType.IsArray;
                    if (Options.Vertical)
                        GUILayout.BeginVertical();
                    else
                        GUILayout.BeginHorizontal();
                    if (field.FieldType.IsArray)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                        GUILayout.Space(Scale(5));
                        if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                        {
                            Array.Resize(ref values, Math.Min(values.Length + 1, int.MaxValue));
                            values[values.Length - 1] = Convert.ChangeType("0", elementType);
                            _changed = true;
                            changed = true;
                        }
                        if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                        {
                            Array.Resize(ref values, Math.Max(values.Length - 1, 0));
                            if (values.Length > 0)
                                values[values.Length - 1] = Convert.ChangeType("0", elementType);
                            _changed = true;
                            changed = true;
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                    }
                    if (!Options.Vertical)
                        GUILayout.Space(Scale(5));

                    if (values.Length > 0)
                    {
                        var isFloat = field.FieldType == typeof(float) || field.FieldType == typeof(double) || field.FieldType == typeof(float[]) || field.FieldType == typeof(double[]);
                        for (int i = 0; i < values.Length; i++)
                        {
                            var val = values[i].ToString();
                            if (Options.Precision >= 0 && isFloat)
                            {
                                if (Double.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo, out var num))
                                {
                                    val = num.ToString($"f{Options.Precision}");
                                }
                            }
                            if (field.FieldType.IsArray)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label($"  [{i}] ", GUILayout.ExpandWidth(false));
                            }
                            var result = field.FieldType == typeof(string) ? GUILayout.TextField(val, Options.MaxLength, options.ToArray()) : GUILayout.TextField(val, options.ToArray());
                            if (field.FieldType.IsArray)
                            {
                                GUILayout.EndHorizontal();
                            }
                            if (result != val)
                            {
                                if (string.IsNullOrEmpty(result))
                                {
                                    if (field.FieldType != typeof(string))
                                        result = "0";
                                }
                                else
                                {
                                    if (Double.TryParse(result, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo, out var num))
                                    {
                                        num = Math.Max(num, Options.Min);
                                        num = Math.Min(num, Options.Max);
                                        result = num.ToString();
                                    }
                                    else
                                    {
                                        result = "0";
                                    }
                                }
                                values[i] = Convert.ChangeType(result, elementType);
                                changed = true;
                                _changed = true;
                            }
                        }
                    }
                    if (_changed)
                    {
                        if (field.FieldType.IsArray)
                        {
                            if (elementType == typeof(float))
                                field.SetValue(instance, Array.ConvertAll(values, x => (float)x));
                            else if (elementType == typeof(int))
                                field.SetValue(instance, Array.ConvertAll(values, x => (int)x));
                            else if (elementType == typeof(long))
                                field.SetValue(instance, Array.ConvertAll(values, x => (long)x));
                            else if (elementType == typeof(double))
                                field.SetValue(instance, Array.ConvertAll(values, x => (double)x));
                        }
                        else
                        {
                            field.SetValue(instance, values[0]);
                        }
                    }
                    if (Options.Vertical)
                        GUILayout.EndVertical();
                    else
                        GUILayout.EndHorizontal();
                }
            }
            else if (Options.Type == DrawType.Slider)
            {
                if (!Array.Exists(sliderTypes, x => x == field.FieldType))
                {
                    throw new Exception($"Type {field.FieldType} can't be drawn as {DrawType.Slider}");
                }

                options.Add(Options.Width != 0 ? GUILayout.Width(Options.Width) : GUILayout.Width(Scale(200)));
                options.Add(Options.Height != 0 ? GUILayout.Height(Options.Height) : GUILayout.Height(Scale((int)drawHeight)));
                if (Options.Vertical)
                    GUILayout.BeginVertical();
                else
                    GUILayout.BeginHorizontal();
                GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                if (!Options.Vertical)
                    GUILayout.Space(Scale(5));
                var val = field.GetValue(instance).ToString();
                if (!Double.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo, out var num))
                {
                    num = 0;
                }
                if (Options.Vertical)
                    GUILayout.BeginHorizontal();
                var fnum = (float)num;
                var result = GUILayout.HorizontalSlider(fnum, (float)Options.Min, (float)Options.Max, options.ToArray());
                if (!Options.Vertical)
                    GUILayout.Space(Scale(5));
                GUILayout.Label(result.ToString(), GUILayout.ExpandWidth(false), GUILayout.Height(Scale((int)drawHeight)));
                if (Options.Vertical)
                    GUILayout.EndHorizontal();
                if (Options.Vertical)
                    GUILayout.EndVertical();
                else
                    GUILayout.EndHorizontal();
                if (result != fnum)
                {
                    if ((field.FieldType == typeof(float) || field.FieldType == typeof(double)) && Options.Precision >= 0)
                        result = (float)Math.Round(result, Options.Precision);
                    field.SetValue(instance, Convert.ChangeType(result, field.FieldType));
                    changed = true;
                }
            }
            else if (Options.Type == DrawType.Toggle)
            {
                if (!Array.Exists(toggleTypes, x => x == field.FieldType))
                {
                    throw new Exception($"Type {field.FieldType} can't be drawn as {DrawType.Toggle}");
                }

                options.Add(GUILayout.ExpandWidth(false));
                options.Add(Options.Height != 0 ? GUILayout.Height(Options.Height) : GUILayout.Height(Scale((int)drawHeight)));
                if (Options.Vertical)
                    GUILayout.BeginVertical();
                else
                    GUILayout.BeginHorizontal();
                GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                var val = (bool)field.GetValue(instance);
                var result = GUILayout.Toggle(val, "", options.ToArray());
                if (Options.Vertical)
                    GUILayout.EndVertical();
                else
                    GUILayout.EndHorizontal();
                if (result != val)
                {
                    field.SetValue(instance, Convert.ChangeType(result, field.FieldType));
                    changed = true;
                }
            }
            else if (Options.Type == DrawType.ToggleGroup)
            {
                if (!field.FieldType.IsEnum)
                {
                    throw new Exception($"Type {field.FieldType} can't be drawn as {DrawType.ToggleGroup}");
                }
                if (field.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                {
                    throw new Exception($"Type {field.FieldType}/{DrawType.ToggleGroup} incompatible with Flag attribute.");
                }

                options.Add(GUILayout.ExpandWidth(false));
                options.Add(Options.Height != 0 ? GUILayout.Height(Options.Height) : GUILayout.Height(Scale((int)drawHeight)));
                if (Options.Vertical)
                    GUILayout.BeginVertical();
                else
                    GUILayout.BeginHorizontal();
                GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                if (!Options.Vertical)
                    GUILayout.Space(Scale(5));
                var values = Enum.GetNames(field.FieldType);
                var val = (int)field.GetValue(instance);

                if (ToggleGroup(ref val, values, null, options.ToArray()))
                {
                    var v = Enum.Parse(field.FieldType, values[val]);
                    field.SetValue(instance, v);
                    changed = true;
                }
                if (Options.Vertical)
                    GUILayout.EndVertical();
                else
                    GUILayout.EndHorizontal();
            }
            else if (Options.Type == DrawType.PopupList)
            {
                if (!field.FieldType.IsEnum)
                {
                    throw new Exception($"Type {field.FieldType} can't be drawn as {DrawType.PopupList}");
                }
                if (field.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                {
                    throw new Exception($"Type {field.FieldType}/{DrawType.ToggleGroup} incompatible with Flag attribute.");
                }

                options.Add(GUILayout.ExpandWidth(false));
                options.Add(Options.Height != 0 ? GUILayout.Height(Options.Height) : GUILayout.Height(Scale((int)drawHeight)));
                if (Options.Vertical)
                    GUILayout.BeginVertical();
                else
                    GUILayout.BeginHorizontal();
                GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
                if (!Options.Vertical)
                    GUILayout.Space(Scale(5));
                var values = Enum.GetNames(field.FieldType);
                var val = (int)field.GetValue(instance);
                if (PopupToggleGroup(ref val, values, fieldName, 0, null, options.ToArray()))
                {
                    var v = Enum.Parse(field.FieldType, values[val]);
                    field.SetValue(instance, v);
                    changed = true;
                }
                if (Options.Vertical)
                    GUILayout.EndVertical();
                else
                    GUILayout.EndHorizontal();
            }
            //else if (Options.Type == DrawType.KeyBinding)
            //{
            //    if (field.FieldType != typeof(KeyBinding))
            //    {
            //        throw new Exception($"Type {field.FieldType} can't be drawn as {DrawType.KeyBinding}");
            //    }

            //    if (Options.Vertical)
            //        GUILayout.BeginVertical();
            //    else
            //        GUILayout.BeginHorizontal();
            //    GUILayout.Label(fieldName, GUILayout.ExpandWidth(false));
            //    if (!Options.Vertical)
            //        GUILayout.Space(Scale(5));
            //    var key = (KeyBinding)field.GetValue(container);
            //    if (DrawKeybinding(ref key, fieldName, unique, null, options.ToArray()))
            //    {
            //        field.SetValue(container, key);
            //        changed = true;
            //    }
            //    if (Options.Vertical)
            //    {
            //        GUILayout.EndVertical();
            //    }
            //    else
            //    {
            //        GUILayout.FlexibleSpace();
            //        GUILayout.EndHorizontal();
            //    }
            //}
            return changed;
        }

        /// <summary>
        /// Renders fields [0.18.0]
        /// </summary>
        internal static void DrawFields<T>(ref T container, DrawFieldMask defaultMask, Action onChange = null) where T : new()
        {
            DrawFields<T>(ref container, 0, defaultMask, onChange);
        }

        /// <summary>
        /// Renders fields [0.22.15]
        /// </summary>
        internal static void DrawFields<T>(ref T container, int unique, DrawFieldMask defaultMask, Action onChange = null) where T : new()
        {
            object obj = container;
            var changed = Draw(obj, typeof(T), defaultMask, unique);
            if (changed)
            {
                container = (T)obj;
                if (onChange != null)
                {
                    try
                    {
                        onChange();
                    }
                    catch
                    {
                        //mod.Logger.LogException(e);
                    }
                }
            }
        }
    }

    internal static class DrawHelper
    {
        /// <summary>
        /// Renders fields with mask OnlyDrawAttr. [0.18.0]
        /// </summary>
        internal static void Draw<T>(this T instance) where T : class, IDrawable, new()
        {
            Drawer.DrawFields(ref instance, DrawFieldMask.OnlyDrawAttr, instance.OnChange);
        }

        /// <summary>
        /// Renders fields with mask OnlyDrawAttr. [0.22.15]
        /// </summary>
        internal static void Draw<T>(this T instance, int unique) where T : class, IDrawable, new()
        {
            Drawer.DrawFields(ref instance, unique, DrawFieldMask.OnlyDrawAttr, instance.OnChange);
        }
    }
}
