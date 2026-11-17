# Changelog

Apa yang berubah di setiap versi Morfa. Versi terbaru di paling atas.

## v1.3.0

Penyegaran stabilitas: keluar dari aplikasi membatalkan konversi yang sedang berjalan, job merge melaporkan kegagalan dengan benar, watch folder mencoba masalah sementara, dan ekspor CSV baru ada di Home.

## Fitur baru

- **Ekspor riwayat ke CSV.** Tombol baru di Home menulis konversi terakhir (tanggal, preset, state, sumber, output, kegagalan) ke file CSV UTF-8.

## Perbaikan bug

- **Keluar dari aplikasi kini membatalkan konversi yang berjalan.** Dulu quit meninggalkan job di tengah run, membuat file setengah jadi di path yang terlihat oleh user.
- **Job merge tidak lagi melaporkan nol kegagalan.** Merge yang mengonversi setiap part tetapi gagal menggabungkannya sekarang menggagalkan file-file itu, sehingga batch bisa dicoba lagi dan watch folder tidak menghitungnya sebagai sukses.
- **Batch watch folder yang gagal di-requeue dengan stabilitas baru.** Sampai tiga percobaan per sesi sebelum dihitung gagal.
- **Migrasi settings tidak lagi dilewatkan ketika profiles.json hilang.** Migrasi bisa hilang diam-diam walau schema masih di-bump.
- **Warna danger brush mode gelap mudah dibaca.** Teks putih di merah muda pucat tidak terbaca; danger kini merah solid.
- **Tombol CTA utama muncul kembali.** Class `accent` yang dipakai Install, Launch, retry dan aksi sejenisnya tidak punya style; sekarang tampil sebagai tombol accent berisi.
- **Mengaktifkan context menu dengan uca.xml rusak gagal dengan rapi.** Hanya export Thunar yang gagal, bukan seluruh SetContextMenu.
- **Mematikan context menu tetap berjalan melewati uca.xml rusak.** Entri file manager lain tetap dihapus.
- **Installer Linux tidak lagi mengganti `~/.local/bin/morfa` milik program lain.** Hanya refresh symlink buatan Morfa; sisanya dibiarkan dan dilaporkan.
- **Uninstaller Linux dan macOS tetap berjalan walau Morfa tidak bisa cleanup registrasi.** Sisanya tetap dihapus.
- **Resolusi nama output berhenti setelah 1000 kandidat bernomor.** Folder sumber pathological tidak bisa memutar loop ini selamanya.
- **Tulisan settings memakai share-delete, mencoba ulang rename, dan membersihkan file `.tmp` tersisa saat startup.**
- **Shortcut tanpa password tidak dimunculkan paksa oleh migrasi.** Migrasi pengaturan yang menambahkan kembali trigger kosong dihapus.
- **Entri trigger memperlakukan `.exe` case-insensitively.** `.exe` di bagian mana pun nama sebelumnya dipotong; sekarang hanya di bagian akhir.
- **Jalur media Markdown yang ditulis ulang oleh round-trip Pandoc diganti hanya di target gambar.** String biasa yang kebetulan cocok dengan jalur di dokumen tidak tersentuh.
- **Konversi SVG melepas native Skia picture saat selesai.**
- **Output XHTML hanya menjaga skema link yang dikenal (http/https/mailto/ftp/ftps/tel).** Skema lain tetap teks, bukan menjadi link yang bisa diklik.
- **Output XML menyaring karakter yang dilarang XML 1.0.** Escape unicode rusak di sebuah sel tidak lagi merusak file.
- **FFmpeg concat list memakai forward slash.** Path Windows tidak lagi perlu di-escape ganda.
- **Status watch folder kini berjalan di UI thread.** Sebelumnya state yang dibind ke DataContext mungkin diperbarui dari background thread.

## Peningkatan

- **Thumbnail konversi terakhir menghapus lima persen terlama, bukan menghapus semua.** Cache penuh tidak lagi memuat ulang seluruhnya dari disk; entri yang panas tetap ada.
- **Concurrency planner diurutkan dalam waktu linear; sanitizer memakai satu set karakter frozen yang dipakai ulang.** Alokasi di setiap jalur konversi turun.
- **Schema settings hanya naik, tidak pernah turun.** File settings yang ditulis build lebih baru tetap menyimpan penanda schema-nya saat downgrade.

## v1.2.2

Penyegaran stabilitas: antrian tidak lagi mati di tengah batch, watch folder menghormati pembatalan dan mencoba batch yang gagal, serta beberapa bug konverter yang bisa merusak file diperbaiki.

## Perbaikan bug

- **Konversi tidak lagi berhenti setelah satu file gagal.** Handler job-finished yang menulis history bisa throw dan mematikan worker antrian, membuat semua file berikutnya macet. Handler sekarang dipisahkan.
- **Watch folder berhenti dengan bersih.** Token stop tidak diteruskan ke job konversi, sehingga batch yang berjalan menghalangi shutdown selamanya dan perubahan aturan bisa membuat watcher dobel. Konversi sekarang menghormati token, penerapan aturan diserialisasi, dan race apply yang membocorkan watcher hilang.
- **Watch folder mencoba batch yang gagal.** Batch yang konversinya tidak bisa dimulai (preset dihapus, engine tidak ada) dibuang selamanya. Sekarang di-requeue dengan batas tiga percobaan per file per sesi.
- **File DICOM tidak lagi overflow pixel count.** Baris dikali kolom wrap integer 32-bit untuk gambar besar, membuat guard pixel data tidak berguna dan alokasi bisa gagal diam-diam.
- **Buku KF8 (Kindle) tidak lagi menulis mojibake.** Part file membawa UTF-8 BOM di atas byte Latin-1, sehingga teks non-ASCII tidak terbaca. Sekarang didecode dengan code page dari header.
- **Pixel buffer native dari Magick.NET sekarang dimatikan.** Tiga call site menahan memori native lebih lama dari yang dibutuhkan.
- **Daftar shortcut yang dikosongkan tidak lagi dimunculkan paksa.** Migrasi pengaturan menambahkan kembali shortcut bawaan setiap kali daftarnya kosong, jadi menghapus semua shortcut tidak permanen.
- **Settings dan watch state ditulis dengan temp file unik.** Dua proses (aplikasi dan CLI) bisa menulis ke file `.tmp` yang sama dan menerbitkan file campuran.

## Peningkatan

- **Pengurutan katalog bebas alokasi.** Prioritas konverter dihitung sekali dan list terurut dibangun dalam waktu linear.
- **Sanitasi output path memakai satu set karakter frozen,** menghemat alokasi concat pada setiap output path.
- **Jendela Clip cutter menggunakan satu route planner** untuk semua path yang dijatuhkan, bukan membangun satu per path.
- **API obsolete diganti.** `PlaceholderText` menggantikan `Watermark`; `BoundingBox` menggantikan `Bounds` / `GlyphRectangle` yang dihapus; `cref` memenuhi syarat untuk kejelasan.

## v1.2.1

Morfa membawa slide "Yang baru di Morfa 1.2.0", halaman Tentang yang lebih rapi dan chrome yang lebih lembut, serta menghilangkan contoh uji coba.

## Fitur baru

- **Yang baru di Morfa 1.2.0.** Pembaruan kini memutar tiga slide: bar atas dua baris, halaman general yang bergulir, dan panel yang turun ke bawah konten saat jendela sempit.

## Peningkatan

- **Chrome yang lebih lembut.** Jendela, setiap halaman, dan Setup di-restyle: warna halaman dan permukaan yang lebih hangat, huruf lebih kecil, sudut lebih bulat, bayangan lebih ringan.
- **Contoh uji coba hilang.** Kartu "Konversi contoh" tidak lagi tampil di Home; Morfa membuka area lepas yang bersih.

## Perbaikan bug

- **Pemberitahuan pihak ketiga di Tentang bisa dibaca.** Panelnya membuka selebar kartu penuh: judulnya tidak terpotong dan tabel lisensi tidak lagi wrap satu karakter per baris.

## v1.2.0

Seluruh jendela utama Morfa dan setiap halamannya menjalani desain ulang: navigasi dua tingkat di bar atas, tata letak yang berbeda untuk setiap halaman, dan panel yang menumpuk di bawah konten utama ketika jendela sempit.

## Peningkatan

- **Bar atas dua tingkat.** Sidebar hilang: baris pertama memuat logo, empat grup serta tombol unduhan, GitHub, Sponsor dan versi; baris kedua menampilkan halaman-halaman grup yang dipilih dan mengingat halaman terakhir yang dipakai.
- **Tata letak yang khas per halaman.** Home dibuka dengan drop zone yang lebih besar dan kolom konversi terbaru; editor donut dibangun dengan baris pill profil dan tray palet penuh; preset menjadi galeri kartu di sebelah editor; shortcut dibuka dengan strip status hook dan dua panel bawah; folder pantau menjadi aliran panel; Mesin beralih antara paket opsional dan urutan prioritas; Tema mendapat pratinjau langsung yang tetap terlihat saat kamu menggulir; Umum mendaftar bagian-bagiannya dalam daftar isi; Tentang dibuka dengan hero dan tiga panel berdampingan.
- **Panel menumpuk saat jendela sempit.** Pada lebar minimum, panel samping turun ke bawah konten utama alih-alih menciut atau bergulir ke samping.

## Perbaikan bug

- **Halaman toolbar donut dibuka dari bagian atas.** Tab palet tidak lagi menggulir halaman ke bawah ke tab yang dipilih.


## v1.1.0

Pengguna baru bisa mencoba Morfa dengan contoh bawaan, melewati langkah engine tanpa ribet, dan memindahkan preset mereka antar PC.

## Fitur baru

- **Jalankan pertama dengan sesuatu untuk dicoba.** Sebuah gambar contoh bawaan menunggu di Home: satu klik mengonversinya ke JPEG, sehingga konversi pertama tidak butuh file sendiri.
- **Engine dengan caramu.** Langkah engine kini mengutamakan melewatkannya; kebanyakan format sudah bekerja, dan engine tetap opsional sampai sebuah format benar-benar membutuhkannya.
- **Beberapa cara mensponsori.** Jendela Sponsor kembali dengan lima opsi: GitHub Sponsors, Saweria, SociaBuzz, Bagibagi, dan Buy Me a Coffee.
- **Impor preset yang lebih aman.** Bundle preset simpanan berformat lebih baru kini ditolak dengan pesan yang jelas.

## v1.0.0

Morfa kini tampil dan berperilaku seperti produknya sendiri, bukan seperti leluhur Filee-nya.

## Peningkatan

- **Identitas sendiri, di mana-mana.** Logo, ikon jendela, Setup, dan warna aksen berpindah dari donut dan ungu warisan ke tanda morfa berwarna teal.
- **Sponsor langsung ke GitHub.** Tombol Sponsor sekarang membuka GitHub Sponsors secara langsung, tanpa jendela berisi layanan lain.

## v0.1.1

Logo, ikon jendela, Setup, dan warna aksen kini memakai identitas produk ini sendiri.

## Peningkatan

- **Identitas sendiri, di mana-mana.** Logo, ikon jendela, Setup, dan warna aksen berpindah dari donut dan ungu warisan ke tanda morfa berwarna teal.
- **Sponsor langsung ke GitHub.** Tombol Sponsor sekarang membuka GitHub Sponsors secara langsung, tanpa jendela berisi layanan lain.

## v0.1.0

Rilis mandiri pertama Morfa: konverter file drag-and-drop, digarap ulang dari source Filee.

## Fitur baru

- **Donut di kursor.** Tahan sebuah tombol, tarik file kamu dan jatuhkan ke sebuah format; hanya format yang cocok untuk file itu yang muncul.
- **263 format, tanpa software lain.** Gambar, PDF, Word, Excel, PowerPoint, HWP, e-book, arsip, font, CAD, model 3D dan data geospasial berjalan tanpa Office, Hancom Office atau LibreOffice.
- **Langsung dari browser.** Tarik gambar, tautan ke file atau unduhan dari Chrome, Edge, Firefox atau Safari dengan cara yang sama.
- **Crop sebelum mengonversi.** Slice Crop membuka gambar satu per satu, dan hanya bagian yang dipilih yang disimpan.
- **Potong klip.** Potong klip dari video dengan timeline, musik, fade dan penghapusan bagian sepi.
- **Watch folder.** Konversi setiap file yang masuk ke folder yang kamu pilih.
- **Dua sisi di setiap donut.** Tekan Tab atau putar roda mouse saat donut terbuka untuk menjangkau format dan alat yang lebih jarang.
- **UI English dan Indonesia.** Default English; ganti ke Bahasa Indonesia di Settings.

## Peningkatan

- **Terinspirasi Filee.** Morfa adalah reimplementasi yang terinspirasi dari [Filee](https://github.com/KnifeLemon/Filee), digarap ulang menjadi proyeknya sendiri dengan namespace, struktur dan identitasnya sendiri. Katalog engine converter, pipeline dan desain toolbar donut berasal dari source Filee yang berlisensi MIT.
- **Konversi paralel.** Banyak file dikonversi sekaligus, dan rute multi-langkah (HWP ke HWPX ke DOCX) ditemukan otomatis.
- **Konversi terbaru.** Setiap hasil muncul di Home dengan thumbnail.

