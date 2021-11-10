﻿using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ComputeSharp
{
    /// <summary>
    /// A <see langword="class"/> that contains extension methods for the <see cref="Vector2"/>, <see cref="Vector3"/> and <see cref="Vector4"/> types
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Converts a <see cref="Vector2"/> array into a <see cref="Span{T}"/> with items of type <see cref="Float2"/>
        /// </summary>
        /// <param name="array">The input <see cref="Vector2"/> array</param>
        /// <returns>A <see cref="Span{T}"/> with items of type <see cref="Float2"/>, mapping the original array</returns>
        [Pure]
        public static Span<Float2> AsFloat2Span(this Vector2[] array) => MemoryMarshal.Cast<Vector2, Float2>(array);

        /// <summary>
        /// Converts a <see cref="Vector2"/> <see cref="Span{T}"/> into a <see cref="Span{T}"/> with items of type <see cref="Float2"/>
        /// </summary>
        /// <param name="span">The input <see cref="Vector2"/> <see cref="Span{T}"/></param>
        /// <returns>A <see cref="Span{T}"/> with items of type <see cref="Float2"/>, mapping the original <see cref="Span{T}"/></returns>
        [Pure]
        public static Span<Float2> AsFloat2Span(this Span<Vector2> span) => MemoryMarshal.Cast<Vector2, Float2>(span);

        /// <summary>
        /// Converts a <see cref="Vector3"/> array into a <see cref="Span{T}"/> with items of type <see cref="Float3"/>
        /// </summary>
        /// <param name="array">The input <see cref="Vector3"/> array</param>
        /// <returns>A <see cref="Span{T}"/> with items of type <see cref="Float3"/>, mapping the original array</returns>
        [Pure]
        public static Span<Float3> AsFloat3Span(this Vector3[] array) => MemoryMarshal.Cast<Vector3, Float3>(array);

        /// <summary>
        /// Converts a <see cref="Vector3"/> <see cref="Span{T}"/> into a <see cref="Span{T}"/> with items of type <see cref="Float3"/>
        /// </summary>
        /// <param name="span">The input <see cref="Vector3"/> <see cref="Span{T}"/></param>
        /// <returns>A <see cref="Span{T}"/> with items of type <see cref="Float3"/>, mapping the original <see cref="Span{T}"/></returns>
        [Pure]
        public static Span<Float3> AsFloat3Span(this Span<Vector3> span) => MemoryMarshal.Cast<Vector3, Float3>(span);

        /// <summary>
        /// Converts a <see cref="Vector4"/> array into a <see cref="Span{T}"/> with items of type <see cref="Float4"/>
        /// </summary>
        /// <param name="array">The input <see cref="Vector4"/> array</param>
        /// <returns>A <see cref="Span{T}"/> with items of type <see cref="Float4"/>, mapping the original array</returns>
        [Pure]
        public static Span<Float4> AsFloat4Span(this Vector4[] array) => MemoryMarshal.Cast<Vector4, Float4>(array);

        /// <summary>
        /// Converts a <see cref="Vector4"/> <see cref="Span{T}"/> into a <see cref="Span{T}"/> with items of type <see cref="Float4"/>
        /// </summary>
        /// <param name="span">The input <see cref="Vector4"/> <see cref="Span{T}"/></param>
        /// <returns>A <see cref="Span{T}"/> with items of type <see cref="Float4"/>, mapping the original <see cref="Span{T}"/></returns>
        [Pure]
        public static Span<Float4> AsFloat4Span(this Span<Vector4> span) => MemoryMarshal.Cast<Vector4, Float4>(span);
    }
}
