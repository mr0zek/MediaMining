FROM alpine:3.14 as exifTool

RUN apk add --no-cache perl make \
	&& apk add --no-cache ca-certificates \
	&& update-ca-certificates \
	&& apk add openssl \
	&& cd /tmp \
	&& wget https://exiftool.org/Image-ExifTool-12.38.tar.gz \
	&& tar -zxvf Image-ExifTool-12.38.tar.gz \
	&& cd Image-ExifTool-12.38 \
	&& perl Makefile.PL \
	&& make test \
	&& make install \
	&& cd .. \
	&& rm -rf Image-ExifTool-12.38.tar.gz

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

WORKDIR /app
COPY --from=exifTool /tmp/Image-ExifTool-12.38 /exiftool/

