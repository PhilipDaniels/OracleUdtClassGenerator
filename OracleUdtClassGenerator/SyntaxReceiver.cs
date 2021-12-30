using System;
using Microsoft.CodeAnalysis;

namespace OracleUdtClassGenerator;

class SyntaxReceiver : ISyntaxContextReceiver
{
    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        // We don't use the receiver for anything at the moment.
        try
        {
        }
        catch (Exception ex)
        {
            Logger.Log("Error parsing syntax: " + ex.ToString());
        }
    }
}