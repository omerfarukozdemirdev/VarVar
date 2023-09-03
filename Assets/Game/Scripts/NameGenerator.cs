using UnityEngine;
using System.Collections.Generic;

public class NameGenerator
{
    private static List<string> maleNames = new List<string>
    {
        "Mehmet", "Ahmet", "Ali", "Mustafa", "Hüseyin", "İbrahim", "Murat",
        "Cemal", "Halil", "Selim", "Süleyman", "Kemal", "Metin", "Uğur", "Yusuf",
        "Emre", "Tarık", "Levent", "Burak", "Can", "Ferhat", "Gökhan", "Hakan",
        "İsmail", "Jemal", "Kadir", "Lütfi", "Mert", "Necip", "Oğuz", "Poyraz",
        "Rıza", "Samet", "Tolga", "Ufuk", "Vedat", "Yasin", "Zafer", "Abdullah",
        "Berkay", "Çetin", "Doruk", "Efe", "Fırat", "Gürkan", "Hasan", "İlker",
        "Javid", "Koray", "Lider", "Mazlum", "Nihat", "Orhan", "Recep",
        "Serkan", "Tayfun", "Utku", "Volkan", "Yavuz", "Zeki",

        "Apo", "Memo", "Çeto", "Tayyar", "Şevko", "Beko", "Tayfun", "Selo",
        "Cango", "Şişko", "Zıpçıko", "Küçük",
        "Tutku", "Kahraman", "Aslan", "Yavuz", "Civan", "Deli",
        "Kızıl", "Ocak", "Yıldırım", "Samuray", "Gökkuşağı", "Alaz", "Kral",
        "Kaptan", "Sakar", "Şanslı", "Zıpır", "Okyanus", "Prens", "Ateş",
        "Panter", "Mucize", "Hızır", "Poyraz", "Mars", "Rüzgar", "Serçe",
        "Fırtına", "Yıldız", "Kara", "Zorro", "Şahin", "Kumru", "Uçan", "Rüzgar",
        "Cesur", "Maraton", "Titan", "Fırat", "Bomba", "Kobra",
        "Kaptan", "Bambu", "Panter", "Deniz", "Kaya", "Çınar", "Ay",
        "Cıvık", "Tersyüz", "Akıl", "Akıncı", "Börü", "Büyük",
        "Kaplumbağa", "Çetin", "Ejderha", "Küçük", "Macera", "Kızıl", "Rüya",
        "Büyücü", "Akrobat", "Usta", "Şen", "Deli", "Akıllı", "Zeki", "Çılgın",
        "Yakışıklı", "Gizemli", "Dost", "Düşman", "Sakar", "Sinsi",
        "Gürültü", "Şamata", "Fırtına", "Deniz", "Kaptan", "Kral", "Samuray"
    };

    private static List<string> femaleNames = new List<string>
    {
        "Ayşe", "Fatma", "Zeynep", "Emine", "Havva", "Elif", "Meryem", "Esra",
        "Hülya", "Sibel", "Gül", "Nur", "Seda", "Aysel", "Leyla", "Cemile",
        "Aslı", "Nilgün", "Derya", "Dilara", "Ebru", "Feride", "Gizem", "Hacer",
        "İrem", "Jale", "Kader", "Leman", "Melek", "Nergis", "Özlem", "Pelin",
        "Rabia", "Selin", "Tülay", "Umay", "Vildan", "Yasemin", "Zerrin", "Ahu",
        "Banu", "Canan", "Deniz", "Eylem", "Feraye", "Gonca", "Hanife", "İlknur",
        "Kamile", "Lale", "Melis", "Nevin", "Oya", "Pınar", "Rana", "Sare",
        "Tansu", "Vedia", "Yelda", "Zuhal", "Alya", "Büşra", "Cansu",
        "Defne", "Elanur", "Funda", "Gamze", "Hilal", "İlayda", "Kamuran", "Leman",
        "Melike", "Nermin", "Oylum", "Perihan", "Rabia", "Sema", "Tuba", "Ufuk",
        "Vesile", "Yaprak", "Zara",

        "Gül", "Lale", "Sultan", "Gazella", "Kardelen", "Çiçek", "Peri", "Fırtına",
        "Yıldız", "Papatya", "Güneş", "Şeker", "Rüzgar", "Rüya", "Bebek", "Melek",
        "Deniz", "Bambu", "Beyaz", "Mavi", "Kırmızı", "Mor", "Yeşil", "Pembe",
        "Sarı", "Turuncu", "Bordo", "Kahverengi", "Lacivert", "Camgöbeği", "Buz",
        "Zümrüt", "Eflatun", "Mercan", "Porselen", "Seramik", "Plastik", "Metal",
        "Taş", "Ahududu", "Kivi", "Mango", "Portakal", "Çilek", "Karpuz", "Vişne",
        "Armut", "Elma", "Muz", "Üzüm", "Kiraz", "Nar", "Şeftali", "Erik",
        "Kız", "Kadın", "Hanım", "Bayan", "Gelin", "Çocuk", "Genç", "Yaşlı",
        "Abla", "Kardeş", "Anne", "Nine",
        "Hala", "Teyze", "Yeğen", "Kuzen", "Akraba", "Dost", "Düşman",
        "Öğretmen", "Doktor", "Avukat", "Sanatçı", "Mimar", "Yazar",
        "Oyuncu", "Şair", "Şarkıcı", "Müzisyen", "Dansçı", "Güzel"
    };

    private List<string> maleNamesList;
    private List<string> femaleNamesList;


    public NameGenerator()
    {
        maleNamesList = new List<string>(maleNames);
        femaleNamesList = new List<string>(femaleNames);
    }


    private string GetRandomNumber()
    {
        int digits = Random.Range(1, 5); // Generate a random number of digits between 1 and 4
        int maxNumber = (int)Mathf.Pow(10, digits) - 1;
        return Random.Range(0, maxNumber + 1).ToString();
    }

    public string GetRandomMaleName()
    {
        int randomIndex = Random.Range(0, maleNamesList.Count);
        string name = maleNamesList[randomIndex] + GetRandomNumber();
        maleNamesList.RemoveAt(randomIndex);
        return name;
    }

    public string GetRandomFemaleName()
    {
        int randomIndex = Random.Range(0, femaleNamesList.Count);
        string name = femaleNamesList[randomIndex] + GetRandomNumber();
        femaleNamesList.RemoveAt(randomIndex);
        return name;
    }

}