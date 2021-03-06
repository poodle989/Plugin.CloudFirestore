﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Android.Runtime;
using Firebase.Firestore;

namespace Plugin.CloudFirestore
{
    public class CollectionReferenceWrapper : ICollectionReference
    {
        private readonly CollectionReference _collectionReference;

        public CollectionReferenceWrapper(CollectionReference collectionReference)
        {
            _collectionReference = collectionReference;
        }

        public IQuery LimitTo(int limit)
        {
            var query = _collectionReference.Limit(limit);
            return new QueryWrapper(query);
        }

        public IQuery OrderBy(string field, bool descending)
        {
            var direction = descending ? Query.Direction.Descending : Query.Direction.Ascending;

            var query = _collectionReference.OrderBy(field, direction);
            return new QueryWrapper(query);
        }

        public IQuery WhereEqualsTo<T>(string field, T value)
        {
            var query = _collectionReference.WhereEqualTo(field, value.ToNativeFieldValue());
            return new QueryWrapper(query);
        }

        public IQuery WhereGreaterThan<T>(string field, T value)
        {
            var query = _collectionReference.WhereGreaterThan(field, value.ToNativeFieldValue());
            return new QueryWrapper(query);
        }

        public IQuery WhereGreaterThanOrEqualsTo<T>(string field, T value)
        {
            var query = _collectionReference.WhereGreaterThanOrEqualTo(field, value.ToNativeFieldValue());
            return new QueryWrapper(query);
        }

        public IQuery WhereLessThan<T>(string field, T value)
        {
            var query = _collectionReference.WhereLessThan(field, value.ToNativeFieldValue());
            return new QueryWrapper(query);
        }

        public IQuery WhereLessThanOrEqualsTo<T>(string field, T value)
        {
            var query = _collectionReference.WhereLessThanOrEqualTo(field, value.ToNativeFieldValue());
            return new QueryWrapper(query);
        }

        public IQuery StartAt(IDocumentSnapshot document)
        {
            var wrapper = (DocumentSnapshotWrapper)document;
            var query = _collectionReference.StartAt((DocumentSnapshot)wrapper);
            return new QueryWrapper(query);
        }

        public IQuery StartAt<T>(IEnumerable<T> fieldValues)
        {
            var query = _collectionReference.StartAt(fieldValues.Select(x => x.ToNativeFieldValue()).ToArray());
            return new QueryWrapper(query);
        }

        public IQuery StartAfter(IDocumentSnapshot document)
        {
            var wrapper = (DocumentSnapshotWrapper)document;
            var query = _collectionReference.StartAfter((DocumentSnapshot)wrapper);
            return new QueryWrapper(query);
        }

        public IQuery StartAfter<T>(IEnumerable<T> fieldValues)
        {
            var query = _collectionReference.StartAfter(fieldValues.Select(x => x.ToNativeFieldValue()).ToArray());
            return new QueryWrapper(query);
        }

        public IQuery EndAt(IDocumentSnapshot document)
        {
            var wrapper = (DocumentSnapshotWrapper)document;
            var query = _collectionReference.EndAt((DocumentSnapshot)wrapper);
            return new QueryWrapper(query);
        }

        public IQuery EndAt<T>(IEnumerable<T> fieldValues)
        {
            var query = _collectionReference.EndAt(fieldValues.Select(x => x.ToNativeFieldValue()).ToArray());
            return new QueryWrapper(query);
        }

        public IQuery EndBefore(IDocumentSnapshot document)
        {
            var wrapper = (DocumentSnapshotWrapper)document;
            var query = _collectionReference.EndBefore((DocumentSnapshot)wrapper);
            return new QueryWrapper(query);
        }

        public IQuery EndBefore<T>(IEnumerable<T> fieldValues)
        {
            var query = _collectionReference.EndBefore(fieldValues.Select(x => x.ToNativeFieldValue()).ToArray());
            return new QueryWrapper(query);
        }

        public IDocumentReference GetDocument(string documentPath)
        {
            var doccuntReference = _collectionReference.Document(documentPath);
            return new DocumentReferenceWrapper(doccuntReference);
        }

        public void GetDocuments(QuerySnapshotHandler handler)
        {
            _collectionReference.Get().AddOnCompleteListener(new OnCompleteHandlerListener((task) =>
            {
                var snapshot = !task.IsSuccessful ? null : task.Result.JavaCast<QuerySnapshot>();
                handler?.Invoke(snapshot == null ? null : new QuerySnapshotWrapper(snapshot),
                                task.IsSuccessful ? null : ExceptionMapper.Map(task.Exception));
            }));
        }

        public Task<IQuerySnapshot> GetDocumentsAsync()
        {
            var tcs = new TaskCompletionSource<IQuerySnapshot>();

            _collectionReference.Get().AddOnCompleteListener(new OnCompleteHandlerListener((task) =>
            {
                if (task.IsSuccessful)
                {
                    var snapshot = task.Result.JavaCast<QuerySnapshot>();
                    tcs.SetResult(snapshot == null ? null : new QuerySnapshotWrapper(snapshot));
                }
                else
                {
                    tcs.SetException(ExceptionMapper.Map(task.Exception));
                }
            }));

            return tcs.Task;
        }

        public void AddDocument<T>(T data, CompletionHandler handler) where T : class
        {
            _collectionReference.Add(data.ToNativeFieldValues())
                                .AddOnCompleteListener(new OnCompleteHandlerListener((task) =>
                                {
                                    handler?.Invoke(task.IsSuccessful ? null : ExceptionMapper.Map(task.Exception));
                                }));
        }

        public Task AddDocumentAsync<T>(T data) where T : class
        {
            var tcs = new TaskCompletionSource<bool>();

            _collectionReference.Add(data.ToNativeFieldValues())
                                .AddOnCompleteListener(new OnCompleteHandlerListener((task) =>
                                {
                                    if (task.IsSuccessful)
                                    {
                                        tcs.SetResult(true);
                                    }
                                    else
                                    {
                                        tcs.SetException(ExceptionMapper.Map(task.Exception));
                                    }
                                }));

            return tcs.Task;
        }

        public IListenerRegistration AddSnapshotListener(QuerySnapshotHandler listener)
        {
            var registration = _collectionReference.AddSnapshotListener(new EventHandlerListener<QuerySnapshot>((value, error) =>
            {
                listener?.Invoke(value == null ? null : new QuerySnapshotWrapper(value),
                                 error == null ? null : ExceptionMapper.Map(error));
            }));

            return new ListenerRegistrationWrapper(registration);
        }

        public IListenerRegistration AddSnapshotListener(bool includeMetadataChanges, QuerySnapshotHandler listener)
        {
            if (!includeMetadataChanges)
            {
                return AddSnapshotListener(listener);
            }

            var option = new QueryListenOptions().IncludeQueryMetadataChanges();

            var registration = _collectionReference.AddSnapshotListener(option, new EventHandlerListener<QuerySnapshot>((value, error) =>
            {
                listener?.Invoke(value == null ? null : new QuerySnapshotWrapper(value),
                                 error == null ? null : ExceptionMapper.Map(error));
            }));

            return new ListenerRegistrationWrapper(registration);
        }
    }
}
