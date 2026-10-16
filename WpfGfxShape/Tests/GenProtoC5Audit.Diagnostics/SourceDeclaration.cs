internal sealed record SourceDeclaration(int Line, string Reference, string Condition, string Text, string Kind);
internal sealed record DirectEdge(string Configuration, string Source, int Line, string Target, string Evidence);
