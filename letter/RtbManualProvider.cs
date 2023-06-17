using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;

namespace Dzik.letter
{
    internal static class RtbManualProvider
    {
        internal static void DisplayXamlMessageManualIn(RichTextBox inboundRtb)
        {
            var title = new Run("Listy - krótka instrukcja");
            var titleParagraph = new Paragraph(title);
            titleParagraph.Foreground = Brushes.Aqua;
            titleParagraph.TextAlignment = TextAlignment.Center;
            titleParagraph.FontSize = 18;
            inboundRtb.Document.Blocks.Add(titleParagraph);

            var basicScreenInfo = new Run("Po lewej stronie znajduje się wiadomość od Twojego rozmówcy, natomiast Ty piszesz po prawej :)\n\n'Listy' przesyła się w butelkach (plikach o tej nazwie). Taką butelkę odbiorca musi jedynie upuścić na otwarte okno Dzika. Butelki są szyfrowane.\n\nWarty uwagi jest przycisk 'Zapisz' który chroni przed przypadkową utratą tego, co się napisało. Aby odczytać zapisany stan i kontynuować edycję, wystarczy ponownie otworzyć okno 'listów'.\n\nObrazki wkleja się albo skrótem ctrl + v, albo przeciągając plik/obrazek na edytor.\nKlik na zdjęcie otwiera jego pełnoekranowy widok. Kolejny klik zamyka widok.\nRozmiar miniaturek wklejanych zdjęć można zaś ustawiać na gónym pasku narzędzi (od 1/1 do 1/4 szerokości pola tekstowego).\n\nW razie potrzeby przesłania więcej niż kilku zdjęć - najlepiej zaszyfrować je osobno, upuszczając pliki na główne okno Dzika.");
            var paragraph = new Paragraph(basicScreenInfo);
            inboundRtb.Document.Blocks.Add(paragraph);

            var colorsNote = new Run("Tekst w 'Listach' ma pewne opcje formatowania, działają skróty takie jak ctrl+b, ctrl+i etc, górny pasek narzędzi pozwala zaś np. zmienić kolor czcionki.\nAby podejrzeć funkcję pełnioną przez dany przycisk na górnym pasku, zatrzymaj kursor myszy na tym przycisku przez kilka sekund, aż pojawi się podpowiedź.\n\nGdyby formatowanie tekstu dawało się we znaki - warto skorzystać z przycisku usuwania formatowania (lub skrótu: ctrl + e), usuwa on również ramki cytatów. SHIFT+ENTER pozwala zaś dodać nowy wiersz tuż pod obecnym, zamiast w nowym paragrafie.");
            var colorsNoteParagraph = new Paragraph(colorsNote);
            colorsNoteParagraph.Foreground = Brushes.LightSkyBlue;
            inboundRtb.Document.Blocks.Add(colorsNoteParagraph);
        }
    }
}
