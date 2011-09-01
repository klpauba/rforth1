: spi-write-read-byte ( b/w -- b' )
    w> SSPBUF c@ drop ( clear BF flag )
    SSPBUF c!
    begin BF bit-set? until
    SSPBUF c@ >w
; inw outw

: spi-write-byte ( b/w -- )
    w> spi-write-read-byte drop
; inw

: spi-read-byte ( -- b/w )
    0 spi-write-read-byte >w	( send dummy byte to receive a byte )
; outw

