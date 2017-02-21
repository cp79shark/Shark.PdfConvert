using NSpec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Shark.PdfConvert.test
{
    public class Samples : nspec
    {
        void Given_a_sample_conversion_settings_with_static_content()
        {
            context["When performing the PDF Conversion of the provided HTML"] = () =>
            {
                beforeEach = () =>
                {
                    PdfConvert.Convert(new PdfConversionSettings
                    {
                        Title = "My Static Content",
                        Content = @"<h1>Lorem ipsum dolor sit amet consectetuer adipiscing elit I SHOULD BE RED BY JAVASCRIPT</h1><script>document.querySelector('h1').style.color = 'rgb(128,0,0)';</script>",
                        OutputPath = @"C:\temp\temp.pdf"
                    });
                };

                it["Should have created a PDF document in the Temp folder"] = () => File.Exists(@"C:\temp\temp.pdf").ShouldBeTrue();
            };
        }

        void Given_a_sample_conversion_settings_with_http_based_content()
        {
            context["When performing the PDF Conversion of the provided HTML from http://www.lipsum.com"] = () =>
            {
                beforeEach = () =>
                {
                    PdfConvert.Convert(new PdfConversionSettings
                    {
                        Title = "My Static Content from URL",
                        ContentUrl = "http://www.lipsum.com/",
                        OutputPath = @"C:\temp\temp-url.pdf"
                    });
                };

                it["Should have created a PDF document in the Temp folder"] = () => File.Exists(@"C:\temp\temp-url.pdf").ShouldBeTrue();
            };
        }

        void Given_a_sample_conversion_settings_with_streamed_content_and_output()
        {
            context["When performing the PDF Conversion of the provided HTML from and to a stream"] = () =>
            {
                beforeEach = () =>
                {
                    PdfConversionSettings config = new PdfConversionSettings
                    {
                        Title = "Streaming my HTML to PDF"
                    };

                    using (var fileStream = new FileStream(@"C:\temp\www.google.com.pdf", FileMode.Create))
                    {
                        var task = new System.Net.Http.HttpClient().GetStreamAsync("http://www.google.com");
                        task.Wait();

                        using (var inputStream = task.Result)
                        {
                            PdfConvert.Convert(config, fileStream, inputStream);
                        }
                    }
                };

                it["Should have created a PDF document in the Temp folder"] = () => File.Exists(@"C:\temp\www.google.com.pdf").ShouldBeTrue();
            };
        }
    }
}
