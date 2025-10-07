# Salasanakone 🔐

Salasanakone on C#:lla tehty sovellus, johon voi tallentaa salasanoja AES-salattuna.
Käyttäjä voi määrittää pääsalasanan (master password), jolla salatut salasanat voidaan myöhemmin avata.

# 🧭 Toiminta ja käyttötarkoitus

Sovelluksen tarkoituksena on auttaa hallitsemaan salasanoja paikallisesti ja turvallisesti omalla tietokoneellaan.
Kaikki salasanat salataan AES-salauksella, ja niiden purkaminen onnistuu vain oikealla master passwordilla.

# Toiminta kerrottuna lyhyesti:
1. Käyttäjä asettaa ensikerralla master passwordin.
2. Käyttäjä voi luoda salasanoja ja salasanat tallennetaan AES-salattuna tiedostoon.
3. Kun sovellus käynnistetään uudelleen, master passwordia kysytään tietojen purkamista varten.

# 💻 Pääkohdat koodista

## AES-salaus
Salasanat salataan turvallisesti AES:lla:
```
public static string EncryptString(string plainText, byte[] key)
{
    using (Aes aes = Aes.Create())
    {
        aes.Key = key;
        aes.GenerateIV();
        using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
        using (var ms = new MemoryStream())
        {
            ms.Write(aes.IV, 0, aes.IV.Length);
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
```
## Master key kysely
Kun sovellus käynnistyy, kysytään master keytä.
```
string masterPassword = Prompt.ShowDialog("Enter master password:", "Security");
byte[] key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(masterPassword));
```

# 💡 Jatkokehitysideat
1. Pilvitallennus ja synkronointi eri laitteiden välillä
2. Mahdollisuus luoda kategorioita eri palveluille




## 🔽 Lataa ja asenna
Siirry [Releases-sivulle](https://github.com/Raiskale/Storg/releases).
1. Siirry [Releases-sivulle](https://github.com/Raiskale/Storg/releases).
2. Lataa uusin installer.
3. Suorita asennus suorittamalla `Salasanakone.exe`.


🧑‍💻 Kehittäjä

Niilo Räisänen
🌐 [https://niilo.cc](https://niilo.cc/)

💾 GitHub: Raiskale
