TESTCASES = tests/test-suite.cmp tests/balise.fs tests/sensors.cmp \
            tests/commande.cmp tests/test-bitops.cmp tests/pwm.fs \
            tests/test-can.fs tests/test-plusminus.cmp tests/spi-pic.cmp

COMPILER = rforth.py

PREDEFINED = lib/core.fs lib/sfrnames.fs lib/primitives.fs lib/arithmetic.fs \
             lib/tables.fs lib/strings.fs lib/math.fs lib/memory.fs \
             lib/canlib.fs

STARTADDR ?= 0x2000

tests: ${TESTCASES}

update-tests: ${TESTCASES:.cmp=.newref}

clean:
	${RM} *.asm tests/*.asm examples/*.asm examples/engines/*.asm
	${RM} *.hex tests/*.hex examples/*.hex examples/engines/*.hex
	${RM} *.lst tests/*.lst examples/*.lst examples/engines/*.lst
	${RM} *.map tests/*.map examples/*.map examples/engines/*.map
	${RM} *.cod tests/*.cod examples/*.cod examples/engines/*.cod

%.asm %.hex %.lst %.map %.cod: %.fs ${COMPILER} ${PREDEFINED}
	python ${COMPILER} -s ${STARTADDR} $<

%.cmp: %.asm %.ref
	diff -u ${@:.cmp=.ref} ${@:.cmp=.asm}

%.newref: %.asm
	cp -p ${@:.newref=.asm} ${@:.newref=.ref}

%.load: %.hex
	python utils/monitor.py --program $<
