using Markdig;
using Markdig.Syntax;
using Humanizer;

if (args.Length != 2)
{
    Console.Error.WriteLine("usage: MDGen.exe [markdownfile] [application name]");
    Environment.Exit(1);
}

var markdownFilename = args[0];
var applicationName = args[1];

var markdown = Markdown.Parse(File.ReadAllText(markdownFilename));

List<IElement> elements = new();

var currentSection = Section.None;

foreach (var element in markdown)
{
    if (element is HeadingBlock hb)
    {
        var heading = hb.Inline!.FirstChild!.ToString();
        if (heading!.Contains("Services", StringComparison.InvariantCultureIgnoreCase))
        {
            currentSection = Section.Services;
        }
        else if (heading!.Contains("Entities", StringComparison.InvariantCultureIgnoreCase))
        {
            currentSection = Section.Entities;
        }
        else if (heading!.Contains("Events", StringComparison.InvariantCultureIgnoreCase))
        {
            currentSection = Section.Events;
        }
        else if (heading!.Contains("EventHandlers", StringComparison.InvariantCultureIgnoreCase))
        {
            currentSection = Section.EventHandlers;
        }
        else if (heading!.Contains("Exceptions", StringComparison.InvariantCultureIgnoreCase))
        {
            currentSection = Section.Exceptions;
        }
    }

    if (currentSection == Section.None) continue;

    IElement? currentElement = null;
    IAttributeElement? currentAttribute = null;

    if (element is ListBlock listBlock)
    {
        foreach (var listItemBlock in listBlock)
        {            
            foreach (var listItemDescendant in listItemBlock.Descendants())
            {
                var listItem = listItemDescendant as ListBlock;
                // Get the name of the Service/Entity/Whatever
                if (listItemDescendant is ParagraphBlock pb)
                {

                    var line = pb!.Inline!.FirstChild!.ToString();
                    if (line == null) continue;

                    var parseResult = ParseLine(line, listItem?.BulletType ?? '-', currentSection);
                    var name = parseResult.Name;
                    var comment = parseResult.Comment;

                    // Top level list element
                    if (listItemDescendant.Column < 3)
                    {
                        if (currentElement != null)
                        {
                            elements.Add(currentElement);
                            currentAttribute = null;
                        }

                        currentElement = currentSection switch
                        {
                            Section.Services => new Service(name, comment, new List<Method>()),
                            Section.Entities => new DataEntity(name, comment, new List<DataMember>(), new List<Method>()),
                            Section.Exceptions => new Exception(name, comment),
                            Section.Events => new Event(name, comment, new List<DataMember>()),
                            _ => throw new InvalidOperationException()
                        };
                    }
                    // These hardcoded values should probably be fixed
                    else if (listItemDescendant.Column == 6)
                    {
                        if (parseResult.ReturnValue != null
                            && currentElement is IMethodElement me)
                        {
                            var method = new Method(name, parseResult.ReturnValue, comment, new List<Attribute>());
                            currentAttribute = method as IAttributeElement; 
                            me.Methods.Add(method);
                        }
                        else if (parseResult.ReturnValue == null
                            && currentElement is IDataElement de)
                        {
                            var dataMember = new DataMember(name, parseResult.DataType, new List<Attribute>());
                            currentAttribute = dataMember as IAttributeElement;
                            de.Data.Add(dataMember);
                        }
                    }
                    else if (listItemDescendant.Column == 10)
                    {
                        if (currentAttribute is not null)
                        {
                            // This is dirty, but I don't have the energy to disambiguate, the syntax is the same
                            currentAttribute.Attributes.Add(new Attribute(name, parseResult.DataType));
                        }
                    }
                }
            }
        }
    }

    if (currentElement != null)
    {
        elements.Add(currentElement);
        currentElement = null;
    }
}

if (elements.Any()) Directory.CreateDirectory($"src/{applicationName}/Abstractions");
if (elements.Where(e => e is Service).Any()) Directory.CreateDirectory($"src/{applicationName}/Services");
if (elements.Where(e => e is DataEntity).Any()) Directory.CreateDirectory($"src/{applicationName}/Entities");
if (elements.Where(e => e is Event).Any()) Directory.CreateDirectory($"src/{applicationName}/Events");
//if (elements.Where(e => e is Service).Any()) Directory.CreateDirectory($"src/{applicationName}/EventHandlers");

foreach (var element in elements)
{
    Console.WriteLine(element.ToString());
}

;



// Lines are typically Name : Type - Comment
//                     Name -> ReturnValue - Comment
ParseResult ParseLine(string line, char bulletType, Section currentSection)
{
    string? returnType = null;
    string? name = null;
    string? comment = null;
    string? dataType = null;

    var commentSplit = line.Split('-');

    if (commentSplit.Length > 1)
    {
        comment = commentSplit[1].Trim();
    }

    if (commentSplit[0].Contains("=>")
        || bulletType == '*'
        || currentSection == Section.Services)
    {
        var methodSplit = commentSplit[0].Split("=>");
        if (methodSplit.Length > 1)
        {
            returnType = methodSplit[1];
        }
        else
        {
            returnType = "void";
        }
        name = methodSplit[0];
    }

    if (commentSplit[0].Contains(":"))
    {
        var dataTypeSplit = commentSplit[0].Split(":");
        if (dataTypeSplit.Length > 1)
        {
            dataType = dataTypeSplit[1];
        }
        name = dataTypeSplit[0];
    }

    if (name == null) name = commentSplit[0];

    name = name!.Trim().Dehumanize();

    return new ParseResult(name, comment, returnType, dataType);
}

record ParseResult(string Name, string? Comment, string? ReturnValue, string? DataType);

enum Section { None, Services, Entities, Events, EventHandlers, Exceptions };

interface IElement { }

interface IMethodElement
{
    public List<Method> Methods { get; }
}

interface IDataElement
{
    public List<DataMember> Data { get; }
}

interface IAttributeElement
{
    public List<Attribute> Attributes { get; }
}

record Service(string Name, string? Comment, List<Method> Methods) : IElement, IMethodElement;

record DataEntity(string Name, string? Comment, List<DataMember> Data, List<Method> Methods) : IElement, IMethodElement, IDataElement;

record Exception(string Name, string? Comment) : IElement;

record Event(string Name, string? Comment, IEnumerable<DataMember> Data) : IElement;

record DataMember(string Name, string? Type, List<Attribute> Attributes) : IAttributeElement;

record Method(string Name, string ReturnValue, string? Comment, List<Attribute> Attributes) : IAttributeElement;

record Attribute(string Name, string? Value);
