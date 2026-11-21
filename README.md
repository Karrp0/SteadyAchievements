# SteadyAchievements

**Türkçe** · [English](README.en.md)

[ArchiSteamFarm](https://github.com/JustArchiNET/ArchiSteamFarm) (ASF) için bir eklenti. ASF'nin oynadığı oyunlarda Steam başarımlarını **yavaş, doğal görünen bir tempoyla** açar. Ayrıca Türkçe/İngilizce bir web paneli, Discord bildirimleri ve isteğe bağlı otomatik oyun seçme özelliği var.

> [!WARNING]
> Başarımları oynamadan açmak **Steam kurallarına aykırıdır**. Eklentiyi kullanmak tamamen senin sorumluluğundadır. Eklenti **simülasyon modunda** başlar ve sen gerçek moda geçene kadar Steam'e hiçbir şey göndermez.

## Ne yapar?

- **Doğal tempo:** Her oyunun kalan başarımları seçtiğin gün aralığına (varsayılan 30–50 gün) rastgele yayılır. Oyuncuların çoğunun açtığı başarımlar genelde önce, nadir olanlar sonra gelir. Nadir bir başarımdan önceki bekleme daha uzundur.
- **Sadece oynanırken:** Plan sadece oyun gerçekten oynanırken (kart düşürme, `GamesPlayedWhileIdle` ya da otomatik oyun) ve senin belirlediğin **aktif saatlerde** ilerler.
- **Sınırlar:** İki başarım arasında her zaman bir en az süre bırakılır, günlük bir sınır da var. İstersen oyunda belli bir oynama süresi dolmadan hiç başlamaz, çok nadir başarımları da atlayabilir.
- **Güvenlik filtreleri:**
  - VAC'lı oyunlar atlanır. İzin listesi bunu aşamaz.
  - Çevrimiçi / çok oyunculu oyunlar atlanır.
  - **Sahip olmadığın, başarım içeren DLC'si** olan oyunlar atlanır.
  - Geliştirici / yayıncı engeli, kara liste ve izin listesi var. ASF'nin kendi kara listesine de uyar.
  - Sadece oyun sunucusunun açabildiği (korumalı) başarımlara hiç dokunulmaz.
- **Otomatik oyun (isteğe bağlı):** ASF'nin düşüreceği kart kalmadığında kütüphanendeki uygun oyunları kendisi seçip oynatır. Senin oyun listen de oynanmaya devam eder. Belirli aralıklarla kontrolü ASF'ye bırakır ki yeni kartlar kaçmasın.
- **Web paneli** (`/steady/`): oynanan oyunlar ve neden atlandıkları, ilerleyen ve biten oyunlar, sıradaki başarım ve tahmini tarihi, her oyunun başarım listesi, ayarlar ve geçmiş. ASF panelinin görünümünde; tema rengi, karanlık mod, dil ve şifre ASF ile ortak. 9 dil: Türkçe, English, Deutsch, Español, Français, Polski, Português (BR), Русский, 简体中文.
- **Discord:** oyun bitince, istersen her başarımda, günlük özet olarak ve sorun çıkınca haber verir.
- **Gizlilik:** Steam dışında hiçbir sunucuya bağlanmaz. Tek istisna, Discord'u kendin açarsan senin webhook adresin.

## Gereksinimler

- ASF **6.3.x** (6.3.11.7 ile test edildi).
- ASF'nin **generic** sürümü önerilir. İşletim sistemine özel sürümler (`linux-x64`, `win-x64`…) "kırpılmıştır" (trimmed): .NET'in ASF'nin kullanmadığı parçaları çıkarılmıştır, saat dilimi veritabanı da yoktur. Eklenti bu sürümlerde de yüklenir ve panelde bir uyarı gösterir.

## Kurulum

1. [Releases](../../releases) sayfasından `SteadyAchievements.zip` dosyasını indir. İstersen `SHA512SUMS` ile `sha512sum -c SHA512SUMS --ignore-missing` komutuyla doğrula.
2. Zip'teki `SteadyAchievements` klasörünü ASF'nin `plugins/` klasörüne çıkar.
3. ASF'yi yeniden başlat. Kayıtta `SteadyAchievements ... loaded` satırını görmelisin.
4. Tarayıcıda `https://<asf-adresin>/steady/` adresini aç (sondaki `/` gerekli) ve ASF'nin IPC şifresini gir.

## İlk kullanım

1. **Ayarlar** sekmesinde aktif saatleri ve temposu belirle.
2. Birkaç gün **simülasyonda** bırak. Kayıtta `[Steady] [Simülasyon] Açılacaktı: ...` satırlarını, panelde de planları göreceksin.
3. Memnunsan **Gerçek** modunu seçip kaydet. Simülasyonda yapılan ilerleme silinir ve planlar hesabının gerçek durumundan yeniden kurulur.

## Ayarlar

Ayarlar her bot için ayrı tutulur: `config/SteadyAchievements/<bot>.settings.json`. Panelden değiştirmen yeterli, yeniden başlatma gerekmez.

| Ayar | Varsayılan | Açıklama |
|---|---|---|
| `Enabled` | `true` | Eklenti bu bot için açık mı. |
| `Simulation` | `true` | Simülasyonda Steam'e hiçbir şey gönderilmez. |
| `Language` | `tr` | Discord, komut yanıtları ve kayıt dili (`tr` / `en`). |
| `MinDays` / `MaxDays` | `30` / `50` | Bir oyunun başarımlarının yayılacağı gün aralığı. |
| `ActiveHours` | `10:00-01:00` | Başarımların açılabileceği saatler. Gece yarısını geçebilir; başlangıç ve bitiş aynıysa bütün gün. |
| `TimeZone` | sunucu saati | IANA adı, ör. `Europe/Istanbul`. Sadece generic sürümde çalışır. |
| `MaxActiveGames` | `2` | Aynı anda ilerleyen oyun sayısı. |
| `MinGapMinutes` | `20` | İki başarım arasındaki en kısa süre (dakika, bot genelinde). |
| `MaxPerDay` | `8` | Günde en fazla başarım. |
| `MinPlaytimeHours` | `1` | Steam'deki oynama süresi bu kadar saate ulaşmadan oyuna başlanmaz. |
| `SkipRarerThan` | `0` | Oyuncuların % kaçından azının açtığı başarımlar hiç açılmasın (0 = kapalı). |
| `ExcludeVac` | `true` | VAC'lı oyunları atla. |
| `ExcludeOnline` | `true` | Çevrimiçi / çok oyunculu oyunları atla. |
| `ProtectDlc` | `true` | Sahip olmadığın başarımlı DLC'si olan oyunları atla. |
| `RespectAsfBlacklist` | `true` | ASF'nin `Blacklist` ayarındaki oyunları da atla. |
| `Blacklist` / `Allowlist` | `[]` | AppID listeleri. İzin listesi çevrimiçi, DLC ve geliştirici kontrollerini aşar; VAC'ı aşamaz. |
| `BlockedDevelopers` / `BlockedPublishers` | `[]` | Bu geliştiricilerin / yayıncıların oyunlarına dokunma. |
| `AutoPlay` | `false` | ASF boştayken uygun oyunları kendisi seçip oynatır. |
| `AutoPlayRotateHours` | `4` | Otomatik oyunda ASF'nin kart kontrolü için kaç saatte bir ara verileceği. |
| `DiscordWebhook` | boş | Discord webhook adresi. |
| `NotifyCompleted` / `NotifyEachUnlock` / `NotifyDailySummary` / `NotifyProblems` | `true` / `false` / `true` / `true` | Hangi durumlarda bildirim gönderilecek. |
| `DailySummaryHour` | `23` | Günlük özetin gönderileceği saat. |

## Komutlar

ASF komut satırında ya da Steam sohbetinde `steady` (kısaca `sa`) ve ardından:

| Komut | Ne yapar |
|---|---|
| `status` | Durum ve ilerleyen oyunlar. |
| `on` / `off` | Eklentiyi aç / kapat. |
| `sim` / `real` | Simülasyon / gerçek mod. |
| `auto on` / `auto off` | Otomatik oyunu aç / kapat. |
| `plan <appid>` | Bir oyunun planını gösterir. |
| `replan <appid>` | Bir oyunun planını siler; oyun oynanırken yeniden kurulur. |

Sona bot adları eklenebilir, ör. `steady status ASF`. Komutlar ASF'de **Master** yetkisi ister.

## Nasıl çalışır?

Zaman "aktif dakika" ile ölçülür: oyunun aktif saatlerde oynandığı her dakika.

1. **Süre:** `MinDays` ile `MaxDays` arasında rastgele bir gün sayısı, günlük aktif dakikayla çarpılır. Sonra kalan başarımların oranıyla ölçeklenir; yarısı açılmış bir oyuna yaklaşık yarı süre ayrılır.
2. **Sıra:** Her başarıma `1,35 · ln(yüzde) + Gumbel gürültüsü` değerinde bir anahtar verilir ve büyükten küçüğe sıralanır. Bu, yüzdesi yüksek olanları öne çeken ama sabit olmayan bir sıra verir.
3. **Kademeler:** "30 maça gir" ile "100 maça gir" gibi kademeli başarımlar her zaman küçükten büyüğe açılır. Aynı sayaca bağlı olanlar sayacın hedefine göre, aynı cümlenin farklı sayılı halleri de içindeki sayıya göre sıralanır.
4. **Aralıklar:** Bir başarımdan önceki bekleme `1 + ln(100 / yüzde)` ile orantılıdır ve log-normal gürültü eklenir. Aralıklar toplam süreyi dolduracak şekilde ölçeklenir ve hiçbiri `MinGapMinutes` değerinden kısa olmaz.
5. **Her dakika:** Oynanan ve uygun olan oyunlar (en fazla `MaxActiveGames`) bir dakika ilerler. Vakti gelmiş en gecikmiş tek başarım açılır; en kısa aralık ve günlük sınır her zaman korunur.

Bir başarım açılmadan hemen önce Steam'deki güncel durum tekrar okunur. Başarım zaten açıksa (örneğin oyunda açmışsan) atlanır. Steam hata verirse oyun 30 dakika bekletilir; aynı başarım 3 kez başarısız olursa atlanır.

## Derleme

```sh
git clone --recurse-submodules https://github.com/Karrp0/SteadyAchievements.git
cd SteadyAchievements
dotnet test SteadyAchievements.Tests -c Release
dotnet build SteadyAchievements -c Release
```

Çıktı `SteadyAchievements/bin/Release/net10.0/` klasöründe: `SteadyAchievements.dll`, `SteadyAchievements.deps.json` ve `www/`. GitHub Actions her `v*` etiketinde zip'i ve `SHA512SUMS` dosyasını otomatik yayımlar.

## Lisans

[Apache 2.0](LICENSE.txt). ASF de Apache 2.0 lisanslıdır.
