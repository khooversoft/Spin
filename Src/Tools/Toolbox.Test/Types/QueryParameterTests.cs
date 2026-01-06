//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Test.Types;

//public class QueryParameterTests
//{
//    [Fact]
//    public void OnlyQuery()
//    {
//        QueryParameter.Parse("base/path.json").Action(result =>
//        {
//            result.Index.Be(0);
//            result.Count.Be(1000);
//            result.Filter.Be("base/path.json");
//            result.Recurse.BeFalse();
//            result.IncludeFile.BeTrue();
//            result.IncludeFolder.BeFalse();
//            result.BasePath.Be("base/path.json");

//            result.GetMatcher().IsMatch("base/Path.json", false).BeTrue();
//            result.GetMatcher().IsMatch("base/Path.json", true).BeFalse();
//            result.GetMatcher().IsMatch("no", false).BeFalse();
//            result.GetMatcher().IsMatch("base", false).BeFalse();
//            result.GetMatcher().IsMatch("base/Path", false).BeFalse();
//            result.GetMatcher().IsMatch("base/newfolder/Path.json", false).BeFalse();
//        });

//        QueryParameter.Parse("filter=base/path.json").Action(result =>
//        {
//            result.Index.Be(0);
//            result.Count.Be(1000);
//            result.Filter.Be("base/path.json");
//            result.Recurse.BeFalse();
//            result.IncludeFile.BeTrue();
//            result.IncludeFolder.BeFalse();
//            result.BasePath.Be("base/path.json");
//        });
//    }

//    [Fact]
//    public void RecursiveSearchQuery()
//    {
//        QueryParameter.Parse("data/**/*").Action(result =>
//        {
//            result.Index.Be(0);
//            result.Count.Be(1000);
//            result.Filter.Be("data/**/*");
//            result.Recurse.BeTrue();
//            result.IncludeFile.BeTrue();
//            result.IncludeFolder.BeFalse();
//            result.BasePath.Be("data");

//            result.GetMatcher().Action(result =>
//            {
//                result.IsMatch("data", true).BeFalse();
//                result.IsMatch("data/json", false).BeTrue();
//                result.IsMatch("data/file.json", false).BeTrue();
//                result.IsMatch("data/folder1/file1", false).BeTrue();
//                result.IsMatch("data/folder1/folder2/file2.txt", false).BeTrue();

//                result.IsMatch("system", false).BeFalse();
//            });
//        });
//    }

//    [Fact]
//    public void Recursive()
//    {
//        QueryParameter.Parse("base/path/**").Action(result =>
//        {
//            result.Index.Be(0);
//            result.Count.Be(1000);
//            result.Filter.Be("base/path/**");
//            result.Recurse.BeTrue();
//            result.IncludeFile.BeTrue();
//            result.IncludeFolder.BeFalse();
//            result.BasePath.Be("base/path");

//            result.GetMatcher().IsMatch("base/Path/Path.json", false).BeTrue();
//            result.GetMatcher().IsMatch("base/Path/Path.json", true).BeFalse();
//            result.GetMatcher().IsMatch("base/Path/newfolder/Path.json", false).BeTrue();
//            result.GetMatcher().IsMatch("no", false).BeFalse();
//            result.GetMatcher().IsMatch("base", false).BeFalse();
//            result.GetMatcher().IsMatch("base/Path", false).BeFalse();
//            result.GetMatcher().IsMatch("base/Path.json", false).BeFalse();
//            result.GetMatcher().IsMatch("base/Path.json", true).BeFalse();
//        });

//        QueryParameter.Parse("base/path/**.json").Action(result =>
//        {
//            result.Index.Be(0);
//            result.Count.Be(1000);
//            result.Filter.Be("base/path/**.json");
//            result.Recurse.BeTrue();
//            result.IncludeFile.BeTrue();
//            result.IncludeFolder.BeFalse();
//            result.BasePath.Be("base/path");

//            result.GetMatcher().Action(result =>
//            {
//                result.IsMatch("base/Path/Path.json", false).BeTrue();
//                result.IsMatch("base/Path/Path.json", true).BeFalse();
//                result.IsMatch("base/Path/newfolder/Path.json", false).BeTrue();
//                result.IsMatch("no", false).BeFalse();
//                result.IsMatch("base", false).BeFalse();
//                result.IsMatch("base/Path", false).BeFalse();
//                result.IsMatch("base/Path.json", false).BeFalse();
//                result.IsMatch("base/Path.json", true).BeFalse();
//            });
//        });

//        QueryParameter.Parse("base/path/*.json").Action(result =>
//        {
//            result.Index.Be(0);
//            result.Count.Be(1000);
//            result.Filter.Be("base/path/*.json");
//            result.Recurse.BeFalse();
//            result.IncludeFile.BeTrue();
//            result.IncludeFolder.BeFalse();
//            result.BasePath.Be("base/path");

//            result.GetMatcher().Action(result =>
//            {
//                result.IsMatch("base/Path/file.json", false).BeTrue();
//                result.IsMatch("base/Path/file2.json", false).BeTrue();
//                result.IsMatch("base/Path/file2.txt", false).BeFalse();
//                result.IsMatch("base/Path/file.json", true).BeFalse();

//                result.IsMatch("base/Path/newfolder/Path.json", false).BeFalse();
//                result.IsMatch("no", false).BeFalse();
//                result.IsMatch("base", false).BeFalse();
//                result.IsMatch("base/Path", false).BeFalse();
//                result.IsMatch("base/Path.json", false).BeFalse();
//                result.IsMatch("base/Path.json", true).BeFalse();
//            });
//        });
//    }

//    [Fact]
//    public void JournalPass()
//    {
//        QueryParameter.Parse("journal2/data/test/journalentry/journal2__data__test__journalentry__*.test.json").Action(result =>
//        {
//            result.Index.Be(0);
//            result.Count.Be(1000);
//            result.Filter.Be("journal2/data/test/journalentry/journal2__data__test__journalentry__*.test.json");
//            result.Recurse.BeFalse();
//            result.IncludeFile.BeTrue();
//            result.IncludeFolder.BeFalse();
//            result.BasePath.Be("journal2/data/test/journalentry");

//            result.GetMatcher().Action(result =>
//            {
//                result.IsMatch("journal2/data/test/journalentry/journal2__data__test__journalentry__202507_20250702.test.json", false).BeTrue();

//                result.IsMatch("journal2/data/test/journalentry/journal3__data__test__journalentry__202507_20250702.test.json", false).BeFalse();
//            });
//        });
//    }

//    [Fact]
//    public void JournalDatePartition()
//    {
//        QueryParameter.Parse("journal2/data/test/journalentry/**/*.json").Action(result =>
//        {
//            result.Index.Be(0);
//            result.Count.Be(1000);
//            result.Filter.Be("journal2/data/test/journalentry/**/*.json");
//            result.Recurse.BeTrue();
//            result.IncludeFile.BeTrue();
//            result.IncludeFolder.BeFalse();
//            result.BasePath.Be("journal2/data/test/journalentry");

//            result.GetMatcher().Action(result =>
//            {
//                result.IsMatch("journal2/data/test/journalentry/202507/20250702/journal2__data__test__journalentry__202507_20250702.test.json", false).BeTrue();
//                result.IsMatch("journal2/data/test/journalentry/202507/20250703/journal2__data__test__journalentry__202507_20250703.test.json", false).BeTrue();

//                result.IsMatch("journal2/data/test/journalentry2/202507/20250703/journal2__data__test__journalentry__202507_20250703.test.json", false).BeFalse();
//            });
//        });
//    }

//    [Fact]
//    public void FullPropertySet()
//    {
//        QueryParameter result = QueryParameter.Parse("filter=base/path.json;index=1;count=2;recurse=true;includeFile=true;includeFolder=true");

//        result.Index.Be(1);
//        result.Count.Be(2);
//        result.Filter.Be("base/path.json");
//        result.Recurse.BeTrue();
//        result.IncludeFile.BeTrue();
//        result.IncludeFolder.BeTrue();
//        result.BasePath.Be("base/path.json");
//    }
//}
