# variables
modname = AutoPoints
dllname = $(modname).dll
version = $(shell cat modInfo.json | awk '/"version"/ {print $$3}' | head -1 | sed 's/[",]//g')
zipname = $(modname)-$(version).zip
zip_files_extra = *.json
builddir = $(modname)
gamedir = /local/games/Steam/steamapps/common/Colony\ Survival
libdir = $(gamedir)/colonyserver_Data/Managed

$(dllname): src/*.cs
	mcs /target:library -nostdlib -r:$(libdir)/Assembly-CSharp.dll,$(libdir)/UnityEngine.CoreModule.dll,$(libdir)/UnityEngine.TextRenderingModule,$(libdir)/mscorlib.dll,$(libdir)/System.dll,$(libdir)/System.Core.dll,$(libdir)/Steamworks.NET.dll,$(libdir)/Newtonsoft.Json.dll -out:"$(dllname)" -sdk:4 -recurse:'src/*.cs'

$(zipname): $(dllname) $(zip_files_extra)
	$(RM) $(zipname)
	mkdir -p $(builddir)
	cp -r $(dllname) $(zip_files_extra) $(builddir)/
	zip -r $(zipname) $(builddir)
	$(RM) -r $(builddir)

.PHONY: build default clean all zip install serverlog clientlog
build: $(dllname)

zip: $(zipname)

default: build

all: checkjson build zip

clean:
	$(RM) $(dllname) $(zipname)
	$(RM) -r $(builddir)

install: build zip
	$(RM) -r $(gamedir)/gamedata/mods/$(builddir)
	unzip $(zipname) -d $(gamedir)/gamedata/mods

checkjson:
	find . -type f -name "*.json" | while read f; do echo $$f; json_pp <$$f >/dev/null; done

serverlog:
	less $(gamedir)/gamedata/logs/server/$$(ls -1rt $(gamedir)/gamedata/logs/server | tail -1)

clientlog:
	less $(gamedir)/gamedata/logs/client/$$(ls -1rt $(gamedir)/gamedata/logs/client | tail -1)

