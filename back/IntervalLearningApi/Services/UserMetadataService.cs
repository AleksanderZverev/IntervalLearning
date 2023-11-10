using DB;
using DB.Models;
using Domain.Collection;
using Domain.User.ValueObjects;

namespace IntervalLearningApi.Services
{
    public class UserMetadataService
    {
        private readonly ApplicationContext db;

        public UserMetadataService(ApplicationContext db)
        {
            this.db = db;
        }

        public UserMetadataEntity GetMetadata(UserId userId)
        {
            var metadata = db.UserMetadata.Single(m => m.ParentUserId == userId);
            return metadata;
        }

        public void CardStateChanged(UserMetadataEntity metadata, Collection collection, bool? lastState, bool? newState)
        {
            var prevState = ToState(lastState);
            var nextState = ToState(newState);

            UpdateMetadata(prevState, false);
            UpdateMetadata(nextState, true);

            void UpdateMetadata(State state, bool isIncrement)
            {

                switch (state)
                {
                    case State.NotStarted:
                        if (isIncrement)
                        {
                            metadata.NotStartedCards++;
                            //collection.NotStartedCards++;
                        }
                        else
                        {
                            metadata.NotStartedCards--;
                            //collection.NotStartedCards--;
                        }
                        break;
                    case State.Started:
                        if (isIncrement)
                        {
                            metadata.StartedCards++;
                            //collection.StartedCards++;
                        }
                        else
                        {
                            metadata.StartedCards--;
                            //collection.StartedCards--;
                        }
                        break;
                    case State.Finished:
                        if (isIncrement)
                        {
                            metadata.FinishedCards++;
                            //collection.FinishedCards++;
                        }
                        else
                        {
                            metadata.FinishedCards--;
                            //collection.FinishedCards--;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }
        }

        private State ToState(bool? state) =>
            state == null ? State.NotStarted : state == false ? State.Started : State.Finished;

        enum State
        {
            NotStarted,
            Started,
            Finished
        }
    }
}
