# Changelog

Apa yang berubah di setiap versi Morfa. Versi terbaru di paling atas.

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

## Fitur baru

- **Donut di kursor.** Tahan sebuah tombol, tarik file kamu dan jatuhkan ke sebuah format; hanya format yang cocok untuk file itu yang muncul.
- **263 format, tanpa software lain.** Gambar, PDF, Word, Excel, PowerPoint, HWP, e-book, arsip, font, CAD, model 3D dan data geospasial berjalan tanpa Office, Hancom Office atau LibreOffice.
- **Langsung dari browser.** Tarik gambar, tautan ke file atau unduhan dari Chrome, Edge, Firefox atau Safari dengan cara yang sama.
- **Crop sebelum mengonversi.** Slice Crop membuka gambar satu per satu, dan hanya bagian yang dipilih yang disimpan.
- **Potong klip.** Potong klip dari video dengan timeline, musik, fade dan penghapusan bagian sepi.
- **Watch folder.** Konversi setiap file yang masuk ke folder yang kamu pilih.
- **Dua sisi di setiap donut.** Tekan Tab atau putar roda mouse saat donut terbuka untuk menjangkau format dan alat yang lebih jarang.
- **UI English dan Indonesia.** Default English; ganti ke Bahasa Indonesia di Settings.

## Catatan

- **Terinspirasi Filee.** Morfa adalah reimplementasi yang terinspirasi dari [Filee](https://github.com/KnifeLemon/Filee), digarap ulang menjadi proyeknya sendiri dengan namespace, struktur dan identitasnya sendiri. Katalog engine converter, pipeline dan desain toolbar donut berasal dari source Filee yang berlisensi MIT.
- **Mulai dari awal.** v0.1.0 adalah rilis pertama morfa sebagai proyek yang berdiri sendiri.

## Optimisasi

- **Konversi paralel.** Banyak file dikonversi sekaligus, dan rute multi-langkah (HWP ke HWPX ke DOCX) ditemukan otomatis.
- **Konversi terbaru.** Setiap hasil muncul di Home dengan thumbnail.

## Penggunaan

Unduh paket untuk sistem kamu dari halaman [Releases](https://github.com/nekonaru/morfa/releases), atau build dari sumber dengan .NET 10 SDK.

```bash
morfa convert photo.heic --to jpg
morfa watch D:\Inbox --to pdf --move-originals
```
