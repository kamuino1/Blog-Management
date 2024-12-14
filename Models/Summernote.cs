#nullable disable 
namespace App.Models
{
    public class Summernote
    {
        public Summernote(string idEditor, bool loadlibrary = true)
        {
            IdEditor = idEditor;
            Loadlibrary = loadlibrary;
        }

        public string IdEditor { get; set; }
        public bool Loadlibrary { get; set; }

        public int height { get; set; } = 120;

        public string toolbar { get; set; } = @"[
            ['style', ['style']],
            ['font', ['bold', 'underline', 'clear']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'video', 'elfinder']],
            ['view', ['fullscreen', 'codeview', 'help']]
            ]";
    }
}