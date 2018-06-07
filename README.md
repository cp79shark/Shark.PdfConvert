# Shark.PdfConvert

## What is Shark.PdfConvert?

Shark.PdfConvert is a simple .NET Core (also targets net451) wrapper around the [WkHtmlToPdf](http://wkhtmltopdf.org) tool. Most options are exposed via a PdfConversionSettings object, others can be specified by using Custom overrides for the configuration area you want.

Conversion setting defaults are set for a Windows environment and assume you have the WkHTMLToPDF (x64) tool installed. You can override the Path to the tool by overridding **PdfConversionSettings . PdfToolPath**.

**You will need to install/download [WkHtmlToPdf](http://wkhtmltopdf.org), it is not embedded in the NuGet Package**

## Sample 1: Static HTML Content

    PdfConvert.Convert(new PdfConversionSettings
    {
        Title = "My Static Content",
        Content = @"<h1>Lorem ipsum dolor sit amet consectetuer adipiscing elit 
		    I SHOULD BE RED BY JAVASCRIPT</h1>
			<script>document.querySelector('h1').style.color = 'rgb(128,0,0)';</script>",
        OutputPath = @"C:\temp\temp.pdf"
    });

## Sample 2: Get Content from a URL

    PdfConvert.Convert(new PdfConversionSettings
    {
        Title = "My URL based Content",
        ContentUrl = "http://www.lipsum.com/",
        OutputPath = @"C:\temp\temp-url.pdf"
    });

## Sample 3: Use Streams for Output and Input

    PdfConversionSettings config = new PdfConversionSettings
    {
        Title = "Streaming my HTML to PDF"
    };

    using (var fileStream = new FileStream(Path.GetTempFileName() + ".pdf", FileMode.Create))
    {
        var task = new System.Net.Http.HttpClient().GetStreamAsync("http://www.google.com");
        task.Wait();

        using (var inputStream = task.Result)
        {
			PdfConvert.Convert(config, fileStream, inputStream);
		}
	}

## Sample 4: Mix and Match

    PdfConversionSettings config = new PdfConversionSettings
    {
        Title = "A little bit of Everything",
        GenerateToc = true,
        TocHeaderText = "Table of MY Contents",
        PageCoverUrl = "https://blackrockdigital.github.io/startbootstrap-landing-page/",
        ContentUrl = "http://www.lipsum.com/",
        PageHeaderHtml = @"
            <!DOCTYPE html>
            <html><body>
            <div style=""background-color: red; color: white; text-align: center; width: 100vw;"">SECRET SAUCE</div>
            </body></html>"
    };

    using (var fileStream = new FileStream(Path.GetTempFileName() + ".pdf", FileMode.Create))
    {
        PdfConvert.Convert(config, fileStream);
    }

## Sample 5: Usage inside MVC Controller Action

    public IActionResult ConvertToPdf([FromBody] PdfConversionSettings model) 
	{
		// TAKE CARE WHEN Accepting the Conversion Settings from user land, it would be best 
		// to just NOT DO it, accept your own custom model and map the parameters as needed.
		// If you insist, then you could do something like the following to prevent malicious code execution
		// in my testing the Custom*Args members are not a valid attack vector, PdfToolPath certainly is, never* trust
		// the client
	#if DEBUG
        // set path to executable, UNSAFE DEBUG USE ONLY FOR TESTING
        model.PdfToolPath = model.PdfToolPath ?? _host.ContentRootPath + @"\wkhtmltopdf.exe";
	#else
        // set path to executable
        model.PdfToolPath = _host.ContentRootPath + @"\wkhtmltopdf.exe";
	#endif	  

	    if (model.OutputFilename.EndsWith(".pdf") == false) model.OutputFilename = model.OutputFilename + ".pdf";

        var memoryStream = new MemoryStream();
        PdfConvert.Convert(model, memoryStream);
        return new FileContentResult(memoryStream.ToArray(), MimeTypes.Pdf)
        {
            FileDownloadName = model.OutputFileName
        };
	}

## Sample 6: Get Content from multiple URLs

    var settings = new PdfConversionSettings
    {
        Title = "My Content from multiple URLs",
        OutputPath = @"C:\temp\temp-url-multiple.pdf"
    };
    settings.ContentUrls.Add("http://www.lipsum.com/");
    settings.ContentUrls.Add("http://www.google.com/");

    PdfConvert.Convert(settings);

## Sample 7: Zoom and Page Size (Issue [#5](https://github.com/cp79shark/Shark.PdfConvert/issues/5))

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

## Revision History
* **1.0.3** - Fixed Issue [#5](https://github.com/cp79shark/Shark.PdfConvert/issues/5) with Zoom / Page Size options and Fixed Header/Footer/Cover issues.
* **1.0.2** - Added ContentUrls property to PdfConversionSettings to allow for multiple URLs to be specified. Requested via [Issue](https://github.com/cp79shark/Shark.PdfConvert/issues/4).
* **1.0.1** - Spoke to soon, updated the samples, they had a typo, small tweaks in the code, nothing breaking or signature modifying
* **1.0.0** - Should be stable going forward except for any bugs found. Modified Convert method signature to be a bit more sane, Added additional static content options, Added Url overrides if you wanted to have WkHTMLToPDF grab external sites for any portion of the generated document, exposed some process options
* **0.1.0** - Initial Upload