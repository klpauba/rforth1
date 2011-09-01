\
\ ENC28J60 Control Registers (see page 12 of the data sheet)
\

\
\ Since the control registers are 5 bits in length, we'll encode the bank in which they
\ reside in the upper 3 bits.  So, when expressed as 8 bit binary "bbbrrrrr", "bbb" is
\ the bank and "rrrrr" is the register.
\

\ ALL BANKS
0x1b constant EIE
0x1c constant EIR
0x1d constant ESTAT
0x1e constant ECON2	
0x1f constant ECON1

\ BANK 0
0x00 constant ERDPTL
0x01 constant ERDPTH
0x02 constant EWRPTL
0x03 constant EWRPTH
0x04 constant ETXSTL
0x05 constant ETXSTH
0x06 constant ETXNDL
0x07 constant ETXNDH
0x08 constant ERXSTL
0x09 constant ERXSTH
0x0a constant ERXNDL
0x0b constant ERXNDH
0x0c constant ERXRDPTL
0x0d constant ERXRDPTH
0x0e constant ERXWRPTL
0x0f constant ERXWRPTH
0x10 constant EDMASTL
0x11 constant EDMASTH
0x12 constant EDMANDL
0x13 constant EDMANDH
0x14 constant EDMADSTL
0x15 constant EDMADSTH
0x16 constant EDMACSL
0x17 constant EDMACSH

\ BANK 1
0x20 constant EHT0
0x21 constant EHT1
0x22 constant EHT2
0x23 constant EHT3
0x24 constant EHT4
0x25 constant EHT5
0x26 constant EHT6
0x27 constant EHT7
0x28 constant EPMM0
0x29 constant EPPM1
0x2a constant EPPM2
0x2b constant EPPM3
0x2c constant EPPM4
0x2d constant EPPM5
0x2e constant EPPM6
0x2f constant EPPM7
0x30 constant EPMCSL
0x31 constant EPMCSH
0x34 constant EPMOL
0x35 constant EPMOH
0x36 constant EWOLIE
0x37 constant EWOLIR
0x38 constant ERXFCON
0x39 constant EPKTCNT

\ BANK 2
0xc0 constant MACON1
0xc1 constant MACON2
0xc2 constant MACON3
0xc3 constant MACON4
0xc4 constant MABBIPG
0xc6 constant MAIPGL
0xc7 constant MAIPGH
0xc8 constant MACLCON1
0xc9 constant MACLCON2
0xca constant MAMXFLL
0xcb constant MAMXFLH
0xcd constant MAPHSUP
0xd0 constant MICON1
0xd1 constant MICON2
0xd2 constant MICMD
0xd4 constant MIREGADR
0xd6 constant MIWRL
0xd7 constant MIWRH
0xd8 constant MIRDL
0xd9 constant MIRDH

\ BANK 3
0xe0 constant MAADR5
0xe1 constant MAADR6
0xe2 constant MAADR3
0xe3 constant MAADR4
0xe4 constant MAADR1
0xe5 constant MAADR2
0x66 constant EBSTSD
0x67 constant EBSTCON
0x68 constant EBSTCSL
0x69 constant EBSTCSH
0xea constant MISTAT
0x72 constant EREVID
0x75 constant ECOCON
0x77 constant EFLOCON
0x78 constant EPAUSL
0x79 constant EPAUSH

\
\ PHY registers (page 19)
\
0x00 constant PHCON1
0x01 constant PHSTAT1
0x02 constant PHID1
0x03 constant PHID2
0x10 constant PHCON2
0x11 constant PHSTAT2
0x12 constant PHIE
0x13 constant PHIR
0x14 constant PHLCON

\ Interrupts masks (page 67)
0x80 constant EINTIE
0x40 constant EPKTIF
0x20 constant EDMAIF
0x10 constant ELINKIF
0x08 constant ETXIF
0x04 constant EWOLIF
0x02 constant ETXERIF
0x01 constant ERXERIF

\ Interresting bits
0x40 constant PKTDEC	\ (page 16)
0x20 constant DMAST	\ (page 15)
0x10 constant CSUMEN	\ (page 15)
0x08 constant TXRTS	\ (page 15)
0x04 constant RXEN	\ (page 15)
0x64 constant RXRST	\ (page 15)
0x01 constant MIIRD	\ (page 21)
0x01 constant BUSY	\ (page 22)
0x01 constant CLKRDY	\ (page 64)
