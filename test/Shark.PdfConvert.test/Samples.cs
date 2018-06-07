using NSpec;
using NSpec.Assertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Shark.PdfConvert.test
{
    public class Samples : nspec
    {
        void Test_Sample1()
        {
            context["Sample 1: Static HTML"] = () =>
            {
                beforeEach = () =>
                {
                    PdfConvert.Convert(new PdfConversionSettings
                    {
                        Title = "My Static Content",
                        Content = @"<h1>Lorem ipsum dolor sit amet consectetuer adipiscing elit 
	    I SHOULD BE RED BY JAVASCRIPT</h1>
		<script>document.querySelector('h1').style.color = 'rgb(128,0,0)';</script>",
                        OutputPath = @"C:\temp\sample1.pdf"
                    });

                };

                it["Should have created a PDF document in the Temp folder"] = () => File.Exists(@"C:\temp\sample1.pdf").ShouldBeTrue();
            };
        }

        void Test_Sample2()
        {
            context["Sample 2: HTML from URL"] = () =>
            {
                beforeEach = () =>
                {
                    PdfConvert.Convert(new PdfConversionSettings
                    {
                        Title = "My Static Content",
                        Content = @"<h1>Lorem ipsum dolor sit amet consectetuer adipiscing elit 
	    I SHOULD BE RED BY JAVASCRIPT</h1>
		<script>document.querySelector('h1').style.color = 'rgb(128,0,0)';</script>",
                        OutputPath = @"C:\temp\sample2.pdf"
                    });

                };

                it["Should have created a PDF document in the Temp folder"] = () => File.Exists(@"C:\temp\sample2.pdf").ShouldBeTrue();
            };
        }

        void Test_Sample3()
        {
            context["Sample 3: Use Input and Output Streams"] = () =>
            {
                beforeEach = () =>
                {
                    PdfConversionSettings config = new PdfConversionSettings
                    {
                        Title = "Streaming my HTML to PDF"
                    };

                    using (var fileStream = new FileStream(@"C:\temp\sample3.pdf", FileMode.Create))
                    {
                        var task = new System.Net.Http.HttpClient().GetStreamAsync("http://www.google.com");
                        task.Wait();

                        using (var inputStream = task.Result)
                        {
                            PdfConvert.Convert(config, fileStream, inputStream);
                        }
                    }
                };

                it["Should have created a PDF document in the Temp folder"] = () => File.Exists(@"C:\temp\sample3.pdf").ShouldBeTrue();
            };
        }

        void Test_Sample4()
        {
            context["Sample 4: Output Streams and Specify URL for Input and Cover with Table of Contents"] = () =>
            {
                beforeEach = () =>
                {
                    PdfConversionSettings config = new PdfConversionSettings
                    {
                        Title = "A little bit of Everything",
                        GenerateToc = true,
                        TocHeaderText = "Table of MY Contents",
                        PageCoverUrl = "https://www.google.com/?q=cover%20page",
                        ContentUrl = "https://www.google.com/?q=content%20page",
                        PageHeaderHtml = @"
        <!DOCTYPE html>
        <html><body>
        <div style=""background-color: red; color: white; text-align: center; width: 100vw;"">SECRET SAUCE</div>
        </body></html>"
                    };

                    using (var fileStream = new FileStream(@"C:\temp\sample4.pdf", FileMode.Create))
                    {
                        PdfConvert.Convert(config, fileStream);
                    }
                };

                it["Should have created a PDF document in the Temp folder"] = () => File.Exists(@"C:\temp\sample4.pdf").ShouldBeTrue();
            };
        }


        void Test_Sample5()
        {
            context["Sample 5: Static Cover, Header, Footer, and Content"] = () =>
            {
                beforeEach = () =>
                {
                    PdfConvert.Convert(new PdfConversionSettings
                    {
                        Title = "My Static Content",
                        PageCoverHtml = @"<!DOCTYPE html>
<html>
<head></head>
<body>
<div style=""height: 100vh; text-align: center;""><h1>Cover Page</h1></div>
</body></html>",
                        PageHeaderHtml = @"<!DOCTYPE html>
<html>
<head></head>
<body>
<h5>HEADER</h5>
</body></html>",
                        PageFooterHtml = @"<!DOCTYPE html>
<html>
<head></head>
<body>
<h5>FOOTER</h5>
</body></html>",
                        Content = @"<h1>Lorem ipsum dolor sit amet consectetuer adipiscing elit I SHOULD BE RED BY JAVASCRIPT</h1><script>document.querySelector('h1').style.color = 'rgb(128,0,0)';</script>",
                        OutputPath = @"C:\temp\sample5.pdf"
                    });
                };

                it["Should have created a PDF document in the Temp folder"] = () => File.Exists(@"C:\temp\sample5.pdf").ShouldBeTrue();
            };
        }

        void Test_Sample6()
        {
            context["Sample 6: Multiple Content URLs"] = () =>
            {
                beforeEach = () =>
                {
                    var settings = new PdfConversionSettings
                    {
                        Title = "My Static Content from URL",
                        OutputPath = @"C:\temp\sample6.pdf"
                    };

                    settings.ContentUrls.Add("https://www.google.com");
                    settings.ContentUrls.Add("https://www.google.com/?q=another");

                    PdfConvert.Convert(settings);
                };

                it["Should have created a PDF document in the Temp folder"] = () => File.Exists(@"C:\temp\sample6.pdf").ShouldBeTrue();
            };
        }

        void Test_Sample7()
        {
            context["Sample 7: Altering Zoom and Page Size"] = () =>
            {
                beforeEach = () =>
                {
                    PdfConvert.Convert(new PdfConversionSettings
                    {
                        Title = "Converted by Shark.PdfConvert",
                        LowQuality = false,
                        Margins = new PdfPageMargins() { Bottom = 10, Left = 10, Right = 10, Top = 10 },
                        Size = PdfPageSize.A3,
                        Zoom = 3.2f,
                        Content = @"<h1>Lorem ipsum dolor sit amet consectetuer adipiscing elit I SHOULD BE RED BY JAVASCRIPT</h1><script>document.querySelector('h1').style.color = 'rgb(128,0,0)';</script>",
                        OutputPath = @"C:\temp\sample7.pdf"
                    });
                };

                it["Should have created a PDF document in the Temp folder"] = () => File.Exists(@"C:\temp\sample7.pdf").ShouldBeTrue();
            };
        }
    }
}
