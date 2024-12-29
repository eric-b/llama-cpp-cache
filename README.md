[![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)](https://hub.docker.com/r/eric1901/llama-cpp-cache)

# Llama.cpp Cache

This program acts like a reverse proxy to an upstream server assumed to be [llama.cpp](https://github.com/ggerganov/llama.cpp/blob/master/examples/server/README.md).

Requests can have a fixed timeout (incoming request cancellation is ignored).
Responses are cached for POST requests, with a JSON body, unless "stream" flag is enabled in the body.

## Motivations

I run llama.cpp on a cheap, slow, low-resources, server. The way I use it from other programs, it's possible requests are made multiple times. I do not care about chat context, so I prefer to return cached response based on incoming body.

Another use case is when some client applications request llama server, and get timeouts. Those timeouts are not always easy to correctly configure, especially for very long timeout like one hour. If such clients retry failed requests, they may get a successful response from this cache.

## Deployment

You can:

- Compile and run binaries (with .NET SDK).
- Run it with [Kubernetes](examples/kubernetes/) (see folder examples).

## Usage

Once configured and deployed, use base URL of this service instead of the direct URL of llama.cpp server.
