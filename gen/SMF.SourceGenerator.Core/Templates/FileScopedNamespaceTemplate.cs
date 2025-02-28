﻿namespace SMF.SourceGenerator.Core.Templates;

using SMF.SourceGenerator.Core.Templates.Interfaces;
using System.CodeDom.Compiler;
using System.Text;

/// <summary>
/// The file scoped namespace template.
/// </summary>

public record FileScopedNamespaceTemplate(string FileScopedNamespace = "SMF.Addons.Shared")
{
    protected StringBuilder? stringBuilder;
    protected StringWriter? _stringWriter;
    protected IndentedTextWriter? _indentedTextWriter;
    private string _fileName = "";

    /// <summary>
    /// Gets the header doc file name.
    /// </summary>
    public string FileName
    {
        get
        {
            if (_fileName != "") return _fileName;
            _fileName = TypeTemplates.FirstOrDefault().IdentifierName;
            if (_fileName is null)
            {
                _fileName = "NoFileName";
            }
            return _fileName;
        }
        init => _fileName = value;
    }

    /// <summary>
    /// Gets the type templates.
    /// </summary>
    public List<ITemplate> TypeTemplates { get; init; } = new();

    /// <summary>
    /// Gets the type templates as string.
    /// </summary>
    public List<string> StringTypeTemplates { get; init; } = new();

    /// <summary>
    /// Gets the using namespaces.
    /// </summary>
    public List<string> UsingNamespaces { get; init; } = new();


    /// <summary>
    /// Gets the header doc file name.
    /// </summary>
    /// <returns>A string.</returns>
    public static string GetHeaderDocFileName(string fileName)
    {
        return $@"// </autogenerated>

// <copyright file=""{fileName}.cs"" company=""SMF"">
// Copyright (c) Smart Modular FrameWork. All rights reserved.
// </copyright>";
    }

    /// <summary>
    /// Gets the file scoped namespace.
    /// </summary>
    /// <returns>A string.</returns>
    private string GetFileScopedNamespace()
    {
        return "namespace " + FileScopedNamespace + ";";
    }


    /// <summary>
    /// Gets the using namespaces_.
    /// </summary>
    /// <returns>A string.</returns>
    private string? GetUsingNamespaces()
    {
        if (UsingNamespaces!.Count == 0) return null;
        StringBuilder stringBuilder = new();
        foreach (string? usingNamespace in UsingNamespaces)
        {
            stringBuilder.Append("using ").Append(usingNamespace).AppendLine(";");
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Gets the type templates.
    /// </summary>
    /// <returns>A string? .</returns>
    private string? GetTypeTemplates()
    {
        if (TypeTemplates!.Count == 0 && StringTypeTemplates.Count == 0) return null;
        StringBuilder stringBuilder = new();
        if (TypeTemplates.Count > 0)
        {
            // Add Delegate Type Template.
            foreach (var typeTemplate in TypeTemplates.Where(_ => _ is DelegateTypeTemplate))
            {
                stringBuilder.AppendLine(typeTemplate.CreateTemplate().GetTemplate());
                UsingNamespaces.AddRange(typeTemplate.UsingNamespaces);
            }

            // Add Other than Delegates Type Template.
            foreach (var typeTemplate in TypeTemplates.Where(_ => _ is not DelegateTypeTemplate))
            {
                stringBuilder.AppendLine(typeTemplate.CreateTemplate().GetTemplate());
                UsingNamespaces.AddRange(typeTemplate.UsingNamespaces);
            }
        }
        if (StringTypeTemplates.Count > 0)
        {
            foreach (var stringTypeTemplate in StringTypeTemplates)
            {
                stringBuilder.AppendLine(stringTypeTemplate);
            }
        }

        return stringBuilder.ToString().TrimEnd();
    }


    /// <summary>
    /// Generates the template.
    /// </summary>
    public FileScopedNamespaceTemplate CreateTemplate()
    {
        stringBuilder = new();
        _stringWriter = new(stringBuilder);
        _indentedTextWriter = new(_stringWriter);

        ExtractValues(out var headerDocFileName, out var fileScopedNamespace, out string? usingNamespaces, out string? namespaceMembers);
        _indentedTextWriter!.WriteLine(headerDocFileName);
        _indentedTextWriter.WriteLine();
        _indentedTextWriter!.WriteLine(fileScopedNamespace);
        if (usingNamespaces is not null) _indentedTextWriter!.WriteLine(usingNamespaces);
        _indentedTextWriter!.WriteLine();
        if (namespaceMembers.Count() > 0) _indentedTextWriter!.WriteLine(namespaceMembers);
        return this;
    }

    /// <summary>
    /// Extracts the values.
    /// </summary>
    /// <param name="headerDocFileName">The header doc file name.</param>
    /// <param name="fileScopedNamespace">The file scoped namespace.</param>
    /// <param name="usingNamespaces">The using namespaces.</param>
    /// <param name="namespaceMembers">The namespace members.</param>
    private void ExtractValues(out string headerDocFileName, out string fileScopedNamespace, out string? usingNamespaces, out string? namespaceMembers)
    {
        headerDocFileName = GetHeaderDocFileName(FileName);
        fileScopedNamespace = GetFileScopedNamespace();
        namespaceMembers = GetTypeTemplates();
        usingNamespaces = GetUsingNamespaces();
    }

    /// <summary>
    /// Gets the template.
    /// </summary>
    /// <returns>A string.</returns>
    public string GetTemplate()
    {
        return _stringWriter!.ToString();
    }
}
