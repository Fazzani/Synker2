namespace PlaylistManager.Entities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq.Expressions;
    using hfa.PlaylistBaseLibrary.Providers;
    public class Playlist<TMedia> : IDisposable, IOrderedQueryable<TMedia>// where TMedia : Media
    {
        private bool _disposed = false; // to detect redundant calls
        readonly IPlaylistProvider<Playlist<TMedia>, TMedia> _playlistProvider;

        [Required]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        private List<TMedia> _medias;
        readonly private Expression _expression;
        private IQueryProvider _provider;

        #region Ctor

        public Playlist()
        {
            _medias = new List<TMedia>();
            _expression = Expression.Constant(_medias);
            _provider = _medias.AsQueryable().Provider;
        }

        public Playlist(IPlaylistProvider<Playlist<TMedia>, TMedia> playlistProvider)
        {
            _playlistProvider = playlistProvider;
            _provider = _playlistProvider as IQueryProvider;
            _expression = Expression.Constant(this);
            // _medias = _playlistProvider.Pull().ToList();
        }

        public Playlist(IQueryProvider playlistProvider, Expression expression)
        {
            _playlistProvider = playlistProvider as IPlaylistProvider<Playlist<TMedia>, TMedia>;
            _provider = playlistProvider;
            _expression = expression;
            // _medias = _playlistProvider.Pull().ToList();
        }

        public Playlist(TMedia[] medias)
        {
            _medias = new List<TMedia>(medias);
            _expression = Expression.Constant(_medias);
            _provider = medias.AsQueryable().Provider;
        }

        public Playlist(IEnumerable<TMedia> medias)
        {
            _medias = new List<TMedia>(medias);
            _expression = Expression.Constant(_medias);
            _provider = medias.AsQueryable().Provider;
        }

        public Playlist(string name, long id, IEnumerable<TMedia> medias)
        {
            Name = name;
            Id = id;
            _medias = medias.ToList();
            _provider = medias.AsQueryable().Provider;
            _expression = Expression.Constant(_medias);
        }

        #endregion

        public TMedia this[int index]
        {
            get { return _medias[index]; }
            set { _medias[index] = value; }
        }

        public int Count => _medias.Count;

        public bool IsReadOnly => false;

        #region IQueryable

        Type IQueryable.ElementType => typeof(TMedia);

        Expression IQueryable.Expression => _provider == null ? Expression.Constant(_medias.AsQueryable()) : Expression.Constant(this);

        IQueryProvider IQueryable.Provider => _provider ?? _medias.AsQueryable().Provider;

        #endregion

        public void Add(TMedia item) => _medias.Add(item);

        public void Clear() => _medias.Clear();

        public bool Contains(TMedia item) => _medias.Contains(item);

        public void CopyTo(TMedia[] array, int arrayIndex) => _medias.CopyTo(array, arrayIndex);

        public int IndexOf(TMedia item) => _medias.IndexOf(item);

        public void Insert(int index, TMedia item) => _medias.Insert(index, item);

        public bool Remove(TMedia item) => _medias.Remove(item);

        public void RemoveAt(int index) => _medias.RemoveAt(index);

        public IEnumerator<TMedia> GetEnumerator() => ((IEnumerable<TMedia>)_provider.Execute(_expression)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_provider.Execute(_expression)).GetEnumerator();

        public override string ToString() => ToString(false);

        public string ToString(bool verbose = true)
           => verbose ? $"Count : {_medias.Count}{Environment.NewLine}{string.Join(" | ", _medias.Select(x => (x as TvgMedia).Name))}{Environment.NewLine}"
            : $"Count : {_medias.Count}{Environment.NewLine}";

        public void Pull()
        {
            _medias = _playlistProvider.Pull()?.ToList();
        }

        public Task<List<TMedia>> PullAsync(CancellationToken token)
        {
            return _playlistProvider.PullAsync(token).ContinueWith(x => _medias = x.Result?.ToList());
        }

        public void Push(Playlist<TMedia> playlist)
        {
            _playlistProvider.Push(playlist);
        }

        public Task PushAsync(Playlist<TMedia> playlist, CancellationToken token)
        {
            return _playlistProvider.PushAsync(playlist, token);
        }

        public Playlist<TMedia> Sync(Playlist<TMedia> playlist)
        {
            return _playlistProvider.Sync(playlist);
        }

        public Task<Playlist<TMedia>> SyncAsync(Playlist<TMedia> playlist, CancellationToken token)
        {
            return _playlistProvider.SyncAsync(playlist, token);
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    _playlistProvider?.Dispose();
                _disposed = true;
            }
        }

    }
}
