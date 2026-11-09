# Changelog

Apa yang berubah di setiap versi Morfa. Versi terbaru di paling atas.

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

