using PlaylistBaseLibrary.Providers.Linq;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.PlaylistBaseLibrary.Providers
{
    public abstract class FileProvider : PlaylistProvider<Playlist<TvgMedia>, TvgMedia>
    {
        private bool _disposed = false;
        protected Uri _uri;
        MemoryStream _stream = new MemoryStream();

        public override MemoryStream PlaylistStream
        {
            get
            {
                if (_stream.Length == 0 )
                    SetStreamAsync().GetAwaiter().GetResult();
                if (_stream.CanSeek)
                    _stream.Seek(0, SeekOrigin.Begin);
                return _stream;
            }
        }

        protected FileProvider(Uri uri)
        {
            _uri = uri;
        }

        private async Task SetStreamAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var stream = await httpClient.GetStreamAsync(_uri);
                    await stream.CopyToAsync(_stream);
                }
            }
            catch (HttpRequestException reqExp)
            {
                throw new ApplicationException($"Playlist not reachable from {_uri}", reqExp);
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public override object _(Expression expression)
        {
            //if (!IsQueryOverDataSource(expression))
            //    throw new InvalidProgramException("No query over the data source was specified.");

            var res = Pull();
            var IqRes = res.AsQueryable();
            var methodsVisitor = new FunctionFinderExpressionVisitor();
            methodsVisitor.GetMethods(expression);

            if (methodsVisitor["where"] is Expression<Func<TvgMedia, bool>> whereExpression)
                res = IqRes.Where(whereExpression);

            if (methodsVisitor["select"] is LambdaExpression selectExpression)
            {
                Type outputType = expression.Type.GenericTypeArguments.FirstOrDefault();
                var selectMethod = typeof(Enumerable)
                  .GetMethods(BindingFlags.Static | BindingFlags.Public)
                  .Single(mi => mi.Name == "Select" &&
                               mi.GetParameters()[1].ParameterType.GetGenericArguments().Count() == 2)
                  .MakeGenericMethod(new Type[] { typeof(TvgMedia), outputType });

                return selectMethod.Invoke(null, new object[] { IqRes, selectExpression.Compile() });
            }
            //TODO: finish the rest of functions (GroupBy, OrderBy, Take, Distinct, Skip, etc...)
            return IqRes;
        }

        #region Pushing medias into playlist
        /// <summary>
        /// Push medias to playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task PushAsync(Playlist<TvgMedia> playlist, CancellationToken cancellationToken)
        {
            if (playlist == null)
                throw new ArgumentNullException(nameof(playlist));

            var data = await GetDataToPushedAsync(playlist, cancellationToken);
            using (var sw = new StreamWriter(PlaylistStream, Encoding.UTF8, 4096, true))
            {
                await sw.WriteAsync(data);
            }
        }

        protected abstract Task<string> GetDataToPushedAsync(Playlist<TvgMedia> playlist, CancellationToken cancellationToken);
        protected abstract string GetDataToPushed(Playlist<TvgMedia> playlist);

        /// <summary>
        /// Push media to playlist
        /// </summary>
        /// <param name="playlist"></param>
        public override void Push(Playlist<TvgMedia> playlist)
        {
            if (playlist == null)
                throw new ArgumentNullException(nameof(playlist));

            var data = GetDataToPushed(playlist);

            using (var sw = new StreamWriter(PlaylistStream, Encoding.UTF8, 4096, true))
            {
                sw.Write(data);
            }
        }
        #endregion

        #region Pulling media from playlist source

        public override IEnumerable<TvgMedia> Pull()
        {
            using (var streamReader = new StreamReader(PlaylistStream, Encoding.UTF8, false, 4096, true))
            {
                return PullMediasFromProvider(streamReader);
            }
        }

        public override async Task<IEnumerable<TvgMedia>> PullAsync(CancellationToken cancellationToken)
        {
            using (var streamReader = new StreamReader(PlaylistStream, Encoding.UTF8, false, 4096, true))
            {
                return await PullMediasFromProviderAsync(streamReader, cancellationToken);
            }
        }

        protected abstract Task<IEnumerable<TvgMedia>> PullMediasFromProviderAsync(StreamReader streamReader, CancellationToken cancellationToken);
        protected abstract IEnumerable<TvgMedia> PullMediasFromProvider(StreamReader streamReader);

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _stream.Close();
                    _stream.Dispose();
                }
                _disposed = true;
            }
        }
    }

   
}
