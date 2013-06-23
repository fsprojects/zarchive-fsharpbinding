#! /bin/bash -e

# On OSX use Mono's private copy of pkg-config if it exists, see https://github.com/fsharp/fsharp/issues/107
osx_pkg_config=/Library/Frameworks/Mono.framework/Versions/Current/bin/pkg-config
if test -e $osx_pkg_config; then
    PKG_CONFIG=$osx_pkg_config
else
    PKG_CONFIG=`which pkg-config`
fi

MONODIR=`$PKG_CONFIG --variable=libdir mono`/mono/4.0

echo "Assuming Mono root directory." $MONODIR

sed -e "s,INSERT_MONO_BIN,$MONODIR,g" Makefile.orig > Makefile

