\ ENC28J60 Memory Map (page 18)
0x2000	   	      	    constant ENC_RAMSIZE		\ size of ENC buffer memory -- 8K
0x0000			    constant RXBUF_ST		\ Starting RXBUF at 0x000 satisfies ERRATA 5
ENC_RAMSIZE 1 -		    constant RXBUF_ND		\ RX buffer is everything
RXBUF_ND RXBUF_ST - 1 +	    constant RXBUF_SZ		\ RXBUF_ND - RXBUF_ST + 1 -- 6668 bytes

: enc-bank    ( register -- bank )
    w>  0xe0 and 5>> >w
; inw outw

: enc-reg    ( register -- addr )
    w> 0x1f and >w
; inw outw

\ ENC28J60 Opcodes (page 26)
: RCR ( register -- cmd )    ; inline
: RBM ( -- cmd )             0x3a ; inline
: WCR ( register -- cmd )    0b01000000 or ; inline
: WBM ( -- cmd )             0x7a ; inline
: BFS ( register -- cmd )    0b10000000 or ; inline
: BFC ( register -- cmd )    0b10100000 or ; inline
: SRC ( -- cmd )             0xff ; inline

: enc-enable  ( -- )
    DBUG bit-clr
    ETH_CS bit-clr
; inline

: enc-disable ( -- )
    ETH_CS bit-set
    DBUG bit-set
; inline 

: enc-write-cmd ( c register -- )
    enc-enable
    spi-write-byte
    ( c ) spi-write-byte
    enc-disable
;

cvariable last-bank
: enc-switch-bank ( register-name/w -- register/w )
    w>
    dup enc-reg swap enc-bank					( -- register bank )
    over EIE >= if						( -- register bank )
	\
        \ no need to switch bank since the register is
	\ available in all banks
	\
    	drop >w exit						( -- register/w )
    then

    dup last-bank c@ = if					( -- register bank )
	\
        \ no need to switch bank since the register is in the
	\ same bank as the last one accessed
	\
        drop >w exit						( -- register/w )
    then
    dup last-bank c!

    dup 0<> if							( -- register bank )
	\
        \ bank non-zero so we must clear the
	\ bank bits
	\
	0x03 ECON1 enc-reg BFC enc-write-cmd
    then							( -- register bank )
    ( bank ) ECON1 enc-reg BFS enc-write-cmd			( -- register )
    >w								( -- register/w )
; inw outw

: isMACorMII?    ( register/w -- flag/w )
    w> 0b01000000 and 0<> >w
; inw outw
 
: enc-read-reg ( register/w -- c/w )
    w>
    dup isMACorMII? swap					( -- isMACorMII? register )
    enc-switch-bank						( -- isMACorMII? register' )

    enc-enable
    RCR spi-write-byte						( -- isMACorMII? )
    spi-read-byte						( -- isMACorMII? c )
    swap if							( -- c )
	\ MAC or an MII register, send one more dummy byte
	\ and return the value
	drop spi-read-byte
    then
    enc-disable

    >w
; inw outw

: enc-write-reg ( value register -- )
    enc-switch-bank 		 ( -- value register' )
    enc-enable
    WCR enc-write-cmd		 ( -- )
;

: enc-write-phy-reg ( valh vall register -- )
    \ Write the data
    MIREGADR enc-write-reg
    MIWRL enc-write-reg
    MIWRH enc-write-reg

    \ Wait for the MII interface to be ready
    begin
	MISTAT enc-read-reg BUSY and 0=
    until
;

: enc-bit-set ( mask register -- ; Sets bits in a register )
    enc-switch-bank BFS enc-write-cmd
;

: enc-bit-clear ( mask register -- ; Clears bits in a register )
    enc-switch-bank BFC enc-write-cmd
;

: ERRATA2 ( -- )
    1ms-delay
;

: ERRATA14 ( n -- n' )
    dup RXBUF_ST = if
        drop RXBUF_ND
    else
        1-
    then
;

: enc-reset   ( -- )	
    \ reset entire enc28j60, clearing all registers
    enc-enable
    SRC spi-write-byte
    enc-disable
    0 last-bank c!
    ERRATA2
;

cvariable ENCRevID
cvariable last-txbuf
variable npp
: enc-init ( -- )
    enc-reset
    EREVID enc-read-reg ENCRevID c!
    ENCRevID c@ 6 <> if
    	   \ not a enc28j60 Revision B7 part
	   ." \nWARNING: only tested on B7 revision of ENC28J60\n"
    then

    0xc0 ECON1  enc-write-reg	\ hold it in reset

    \ Allocate transmit and receive buffers
    RXBUF_ST 1>2  ERXSTH   enc-write-reg  ERXSTL   enc-write-reg
    RXBUF_ND 1>2  ERXNDH   enc-write-reg  ERXNDL   enc-write-reg
    RXBUF_ST 1>2  ERDPTH   enc-write-reg  ERDPTL   enc-write-reg
    RXBUF_ST 1>2  ERXRDPTH enc-write-reg  ERXRDPTL enc-write-reg
    RXBUF_ST npp !

    \ Wait for the MAC & PHY parts to be operational
    begin
	ESTAT enc-read-reg
    	CLKRDY and 
    until
    
    \ Initialize MAC subsystem
    0x10 MAPHSUP enc-write-reg					\ Release MAC-PHY support from Reset
    1518 1>2 MAMXFLH enc-write-reg MAMXFLL enc-write-reg	\ Maximum frame length (1518 bytes)
    	       	       		 	 			\ We probably want to change this is we can't actually handle it )
    0x15 MABBIPG enc-write-reg					\ IEEE BTB Inter Packet Gap
    0x12 MAIPGL  enc-write-reg					\ IEEE Non BTB Interpacket Gap
    \ DEFAULT: 0x0c MAIPGH  enc-write-reg			\ IEEE Non BTB Inter Packet Gap

    \ Initialize PHY subsystem
    \ 0x00 0x00 PHCON1 enc-write-phy-reg	\ Half duplex
    0x01 0x00 PHCON1 enc-write-phy-reg	\ full duplex
    0x04 0x76 PHLCON enc-write-phy-reg	\ Display link status, display tx and rx activity, stretch to 73ms

    \ Initialization done, release the NIC from reset
    0x01 MACON1  enc-write-reg	\ Enable packet reception
    0x14 ECON1 enc-write-reg

    0 last-txbuf c!
;

create enc-macaddr 0x00 c, 0x04 c, 0xa3 c,	\ OUI -- vendor code for Microchip Technologies (0004a3)
                   0x00 c, 0x00 c, 0x00 c, 	\ interface serial number (000000)
create broadcast-mac 0xff c, 0xff c, 0xff c, 0xff c, 0xff c, 0xff c,
create zero-mac         0 c,    0 c,    0 c,    0 c,    0 c,    0 c,

: enc-set-macaddr ( mac -- )
    \ Set the MAC address
    ( macaddr )
       dup enc-macaddr 6 memcpy		\ save the address for future use
       dup c@ MAADR1 enc-write-reg
    1+ dup c@ MAADR2 enc-write-reg
    1+ dup c@ MAADR3 enc-write-reg
    1+ dup c@ MAADR4 enc-write-reg
    1+ dup c@ MAADR5 enc-write-reg
    1+     c@ MAADR6 enc-write-reg
;

: enc-rd-seek-to ( addr -- )
    \ dup ." RDSEEK:" . cr
    \ see section 7.2.3 -- Example 7-1: random access address calculation
    dup RXBUF_ND > if
        RXBUF_ND RXBUF_ST - 1+ -
    then
    1>2  ERDPTH enc-write-reg  ERDPTL enc-write-reg
;

: enc-rxrd-seek-to ( addr -- )
    \ dup ." RXRDSEEK:" . cr
    dup enc-rd-seek-to			\ set the read pointer to the beginning of the next packet

    \
    \ Free the space for the previous packet
    \
    ERRATA14
    \ dup ." NPP:" . cr
    1>2 swap 
    \ 2dup ." ERXRDPTL:" . cr
    \      ." ERXRDPTH:" . cr
    ERXRDPTL enc-write-reg		\ update the RXRD pointer in the enc -- WRITE LSB FIRST!
    ERXRDPTH enc-write-reg		\ ...
;

: enc-read ( addr len/w -- )
    \ Read sequential 'len' bytes from the data buffer and place into memory at 'addr'
    \ ERDPT must point to the place to read from

    w>
    enc-enable			\ enable CS for enc
    RBM spi-write-byte		\ read enc memory
    cfor
        spi-read-byte over c!	( -- addr )
	1+
    cnext
    enc-disable			\ disable CS for enc
    drop

; inw

: enc-remove-packet ( -- )
	PKTDEC ECON2 enc-reg BFS enc-write-cmd \ decrement the packet counter
;

546 constant ETH_PACKET_MAX_SZ
  6 constant ENC_PREAMBLE_SZ
create enc-buffer ETH_PACKET_MAX_SZ ENC_PREAMBLE_SZ + allot
: enc-npp  ( -- addr ) enc-buffer ;
: enc-npp@ ( -- n ) enc-npp @ ;
: enc-status ( -- addr ) enc-buffer 2 + ;
: enc-packet-length ( -- n ) enc-status @ ;
: enc-packet ( -- addr ) enc-buffer 6 + ;

: enc-receiveOK? ( -- f )
    enc-npp >r
    r@ @ RXBUF_ND >		\ npp out of range ...  ( -- addr f )
    dup if ."    NPP range error:" r@ @ . cr ( DEBUG ) then

    r@ 2 + @
    dup 20 < swap 1518 > or	( -- addr f flen )
    dup if ."    packet size error:" r@ 2 + @ . cr ( DEBUG ) then
    or		\ ... or length out of range

    r@ 4 + c@ 0b00110001 and
    0<>
    dup if ."    1st byte error:" r@ 4 + c@ 0b01110001 and . cr then ( DEBUG )
    or	 		\ ... or 1st set of error flags indicate bad packet
    r@ 5 + c@ 0b01100000 and 
    0<>
    dup if ."    2nd byte error:" r@ 5 + c@ 0b01100000 and . cr then ( DEBUG )
    or			\ ... or 2nd set of error flags indicate bad packet
    0=
    r> drop
;

: enc-receive ( -- length|-1 )
    EPKTCNT enc-read-reg 0> if
        enc-buffer ENC_PREAMBLE_SZ enc-read

	enc-receiveOK? if
	    enc-packet enc-packet-length
	    dup ETH_PACKET_MAX_SZ > if
	    	." PACKET TOO LARGE!"
	    	drop ETH_PACKET_MAX_SZ
	    then
	    enc-read
	    enc-npp@ npp !
	    enc-packet-length
	else
	    ." RESETTING ENC28J60\n"
	    enc-init
    	    enc-macaddr enc-set-macaddr
    	    RXBUF_ST npp !
	    -1
        then

    	npp @ enc-rxrd-seek-to
    	enc-remove-packet
    else
        0
    then
;

: MAC-init enc-init ; inline
: MAC-packet enc-packet ; inline
: MAC-receive ( -- f ) enc-receive ; inline
: MAC-set-macaddr ( addr -- ) enc-set-macaddr ; inline
: MAC-get-macaddr ( -- addr ) enc-macaddr ; inline
: MAC-send ( TODO ) ; inline

